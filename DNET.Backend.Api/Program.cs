using DNET.Backend.Api.Infrastructure;
using DNET.Backend.Api.Options;
using DNET.Backend.Api.Services;
using DNET.Backend.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;
using Winton.Extensions.Configuration.Consul;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConsul("TableReservationsConfig", options =>
{
    options.Optional = true;
    options.ReloadOnChange = true;
    options.OnLoadException = exceptionContext => { exceptionContext.Ignore = true; };
    options.OnWatchException = _ => TimeSpan.FromMinutes(5);
});

builder.Services.Configure<TableOptions>(builder.Configuration.GetSection("TableSettings"));
builder.Services.AddScoped<ITableService, TableService>();

builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("ReservationSettings"));
builder.Services.AddScoped<IReservationService, ReservationService>();

builder.Services.AddSingleton<IJwtValidator, JwtValidator>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.AddScoped<LogMiddleware>();
builder.Services.AddScoped<ExceptionMiddleware>();

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
    .AddJwtBearer(options => { options.TokenValidationParameters = JwtValidator.CreateTokenValidationParameters(); })
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
    
    option.OperationFilter<SwaggerHeaderFilter>();
});

var app = builder.Build();

app.MapControllers();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LogMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<TableReservationsDbContext>();
dbContext.Database.Migrate();

app.Run();

public partial class Program
{
}
