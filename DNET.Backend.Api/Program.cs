using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region API


var tables = new Dictionary<int, Table>();
var reservations = new Dictionary<int, Reservation>();

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


// GET /reservations/1
app.MapGet("/reservations/{id}", (int id) =>
{
    if (!reservations.TryGetValue(id, out var reservation))
        return Results.NotFound(new ErrorResponse { Message = "Reservation not found" });
    
    return Results.Ok(reservation);
});

// GET /reservations?tableId=1&date=2025-02-29
app.MapGet("/reservations", (HttpContext context, int? tableId, DateTime? date) =>
{
    var validParameters = new HashSet<string> { "tableId", "date" };
    var queryParameters = context.Request.Query.Keys;
    foreach (var param in queryParameters)
    {
        if (!validParameters.Contains(param))
        {
            return Results.BadRequest(new ErrorResponse { Message = $"Invalid query paramenter: {param}" });
        }
    }
    
    var filtered = reservations.AsEnumerable();

    if (tableId.HasValue)
        filtered = reservations.Where(x => x.Value.TableId == tableId);
    if (date.HasValue)
        filtered = filtered.Where(x => x.Value.StartTime.Date == date.Value.Date);
        
    var resultList = filtered.Select(x => x.Value).ToList();
    if (resultList.Count == 0)
        return Results.NotFound(new ErrorResponse { Message = "Reservations not found" });

    return Results.Ok(resultList);
});

// POST /reservations
app.MapPost("/reservations", (Reservation reservation) =>
{
    reservations.Add(reservations.Count + 1, reservation);
    return Results.Created($"/reservations/{reservations.Count}", reservation);
});

// PUT /reservations/1
app.MapPut("/reservations/{id}", (int id, Reservation reservation) =>
{
    if (!reservations.TryGetValue(id, out var existingReservation))
        return Results.NotFound(new ErrorResponse { Message = "Reservation not found" });

    existingReservation.TableId = reservation.TableId;
    existingReservation.ClientId = reservation.ClientId;
    existingReservation.StartTime = reservation.StartTime;
    existingReservation.EndTime = reservation.EndTime;
    
    return Results.Ok(existingReservation);
});

// DELETE /reservations/1
app.MapDelete("/reservations/{id}", (int id) =>
{
    if (!reservations.Remove(id))
        return Results.NotFound(new ErrorResponse { Message = "Reservation not found" });
    
    return Results.NoContent();
});

#endregion

app.Run();


public record Table
{
    public int Id { get; set; }
    public int Capacity { get; set; }
}

public record Reservation
{
    public int ClientId { get; set; }
    public int TableId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

class ErrorResponse
{
    public string Message { get; set; }
}

public partial class Program
{
}
