using System.Text.Json;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using DNET.Backend.Api.Services;
using Microsoft.Extensions.Options;
using Winton.Extensions.Configuration.Consul;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConsul("ReservationConfig", options =>
{
    options.Optional = true;
    options.ReloadOnChange = true;
    options.OnLoadException = exceptionContext => { exceptionContext.Ignore = true; };
    options.OnWatchException = _ => TimeSpan.FromMinutes(5);
});

builder.Services.AddControllers();

builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("ReservationSettings"));

builder.Services.AddScoped<IReservationService, ReservationService>();

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

#region API


var tables = new Dictionary<int, Table>();

//GET /tables (with pagination)
app.MapGet("/tables", (int page = 1, int size = 10) =>
{
    var totalItems = tables.Values.Count;
    var paginatedTables = tables.Values
        .Skip((page - 1) * size)
        .Take(size)
        .ToList();
    
    if (paginatedTables.Count == 0)
        return Results.NotFound(new ErrorResponse { Message = "No tables found for the specified page and size." });
    
    var response = new
    {
        TotalItems = totalItems,
        Page = page,
        PageSize = size,
        TotalPages = (int)Math.Ceiling((double)totalItems / size),
        Items = paginatedTables
    };

    return Results.Ok(response);
});


// GET /tables/1
app.MapGet("/tables/{id:int}", (int id) =>
{
    if (!tables.TryGetValue(id, out var table))
        return Results.NotFound(new ErrorResponse { Message = "Table not found" });
    
    return Results.Ok(table);
});


// GET /tables/filter?capacity=4
app.MapGet("/tables/filter/", (int capacity) =>
{
    var filtered = tables.Values
        .Where(x => x.Capacity == capacity)
        .ToList();
    
    if (filtered.Count == 0)
        return Results.NotFound(new ErrorResponse { Message = "Table not found" });
    
    return Results.Ok(filtered);
});


// POST /tables
app.MapPost("/tables", (Table table) =>
{
    if (tables.ContainsKey(table.Id))
        return Results.BadRequest(new ErrorResponse { Message = "A table with this ID already exists." });
    
    tables.Add(tables.Count + 1, table);
    return Results.Created($"/tables/{tables.Count}", table);
});


// PUT /tables/1
app.MapPut("/tables/{id:int}", (int id, Table table) =>
{
    if (!tables.TryGetValue(id, out var existingTable))
        return Results.NotFound(new ErrorResponse { Message = "Table not found" });
    
    if (table.Id != id && tables.ContainsKey(table.Id))
        return Results.BadRequest(new ErrorResponse { Message = "A table with this ID already exists." });
    
    existingTable.Id = table.Id;
    existingTable.Capacity = table.Capacity;
    return Results.Ok(existingTable);
});


// PATCH /tables/1
app.MapPatch("/tables/{id:int}", (int id, JsonElement patch) =>
{
    if (!tables.TryGetValue(id, out var existingTable))
        return Results.NotFound(new ErrorResponse { Message = "Table not found" });
    
    if (patch.TryGetProperty("id", out var idElement))
    {
        var newId = idElement.GetInt32();
        if (newId != id && tables.ContainsKey(newId))
            return Results.BadRequest(new ErrorResponse { Message = "A table with this ID already exists." });
        
        existingTable.Id = newId;
    }
    
    if (patch.TryGetProperty("capacity", out var capacityElement))
        existingTable.Capacity = capacityElement.GetInt32();

    return Results.Ok(existingTable);
});


// DELETE /tables/1
app.MapDelete("/tables/{id:int}", (int id) =>
{
    if (!tables.Remove(id))
        return Results.NotFound(new ErrorResponse { Message = "Table not found" });
  
      return Results.NoContent();
});

#endregion

app.Run();


public record Table
{
    public int Id { get; set; }
    public int Capacity { get; set; }
}

public partial class Program
{
}
