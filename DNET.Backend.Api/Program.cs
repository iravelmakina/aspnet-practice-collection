using DNET.Backend.Api.Options;
using DNET.Backend.Api.Services;
using Winton.Extensions.Configuration.Consul;
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConsul("Table&ReservationConfig", options =>
{
    options.Optional = true;
    options.ReloadOnChange = true;
    options.OnLoadException = exceptionContext => { exceptionContext.Ignore = true; };
    options.OnWatchException = _ => TimeSpan.FromMinutes(5);
});

builder.Services.AddControllers();

builder.Services.Configure<TableOptions>(builder.Configuration.GetSection("TableSettings"));
builder.Services.AddSingleton<ITableService, TableService>();

builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("ReservationSettings"));
builder.Services.AddSingleton<IReservationService, ReservationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

public partial class Program
{
}
