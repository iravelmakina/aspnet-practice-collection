using DNET.Backend.Api.Infrastructure;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using DNET.Backend.Api.Services;
using DNET.Backend.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;
using Winton.Extensions.Configuration.Consul;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using StackExchange.Redis;
using Policy = Polly.Policy;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConsul("TableReservationsConfig", options =>
{
    options.Optional = true;
    options.ReloadOnChange = true;
    options.OnLoadException = exceptionContext => { exceptionContext.Ignore = true; };
    options.OnWatchException = _ => TimeSpan.FromMinutes(5);
});

builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection("RateLimitSettings"));

builder.Services.Configure<TableOptions>(builder.Configuration.GetSection("TableSettings"));
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.Decorate<ITableService, TableServiceWithCache>();

builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("ReservationSettings"));
builder.Services.AddScoped<IReservationService, ReservationService>();

builder.Services.AddSingleton<IJwtValidator, JwtValidator>();
builder.Services.AddScoped<IHttpService, HttpService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<ETagFilter>();
builder.Services.AddScoped<ExceptionMiddleware>();
builder.Services.AddScoped<LogMiddleware>();

builder.Services.AddMemoryCache();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp:";
});

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddSingleton<RateLimitMiddleware>();

builder.Services.AddHostedService<MigrationExecutionStartupService>();
builder.Services.AddHostedService<ResetCodeBackgroundCleaner>();

builder.Services.AddHttpClient("MyHttpClient", client =>
    {
        client.BaseAddress = new Uri("https://webhook.site/c4d008a5-7c49-49b8-a81f-53c60ac4ad41/");
        client.DefaultRequestHeaders.Add("Header", "Value");
    })
    .AddPolicyHandler(GetRetryPolicy(3, 1_000, 2_000))
    .AddPolicyHandler(GetCircuitBreakerPolicy(5, 2_000))
    .AddHttpMessageHandler<HttpClientLogHandler>();

builder.Services.AddTransient<HttpClientLogHandler>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<TimestampHeaderFilter>();
});

builder.Services.AddDbContext<TableReservationsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TableReservationsDb"))
);

builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddGoogle(options =>
    {
        var clientId = builder.Configuration["Authentication:Google:ClientId"] ??
                       throw new ArgumentNullException("ClientId");
        var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ??
                           throw new ArgumentNullException("ClientSecret");

        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = JwtValidator.CreateTokenValidationParameters();
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 403;
                
                return context.Response.WriteAsJsonAsync(new ErrorResponse
                {
                    Status = 403,
                    Message = "You do not have the required role to perform this action"
                });
            },
            OnChallenge = context =>
            {
                context.Response.StatusCode = 401;
                
                return context.Response.WriteAsJsonAsync(new ErrorResponse
                {
                    Status = 401,
                    Message = "Unauthorized. Invalid or expired token"
                });
            }
        };
    })
    .AddCookie();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    
    option.OperationFilter<SwaggerAuthFilter>();
    option.OperationFilter<SwaggerHeaderFilter>();
});

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console(formatter: context.HostingEnvironment.IsDevelopment()
        ? new JsonFormatter()
        : new Serilog.Formatting.Display.MessageTemplateTextFormatter(
            "[{Timestamp:HH:mm:ss} {Level}] {Message}{NewLine}{Exception}"
        ));
});

var app = builder.Build();

app.MapControllers();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LogMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RateLimitMiddleware>();


app.UseSwagger();
app.UseSwaggerUI();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int breakAfter, int breakDurationMs)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .Or<TaskCanceledException>()
        .CircuitBreakerAsync(breakAfter, 
            TimeSpan.FromMilliseconds(breakDurationMs),
            onBreak: (outcome, timespan) =>
            {
                Console.WriteLine($"Circuit broken! Breaking for {timespan.TotalMilliseconds}ms due to: {outcome.Exception?.Message}");
            },
            onReset: () => Console.WriteLine("Circuit reset! Calls are allowed again.")
        );
}


static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retry, int retryAfterMs, int timeout)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .Or<TaskCanceledException>()
        .WaitAndRetryAsync(Enumerable.Range(1, retry).Select(i => TimeSpan.FromMilliseconds(i * retryAfterMs)))
        .WrapAsync(Policy.TimeoutAsync<HttpResponseMessage>(timeout));
}

public partial class Program
{
}
