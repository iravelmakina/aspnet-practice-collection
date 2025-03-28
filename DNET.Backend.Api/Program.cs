using DNET.Backend.Api.Options;
using DNET.Backend.Api.Services;
using DNET.Backend.DataAccess;
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

builder.Services.AddControllers();

builder.Services.AddDbContext<TableReservationsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TableReservationsDb"))
);

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
