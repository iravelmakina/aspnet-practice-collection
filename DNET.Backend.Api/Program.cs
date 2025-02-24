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

app.MapGet("/", () => "Hello World!");

// // GET /products
app.MapGet("/products", () => { return Results.Ok(new[] { new Product { Name = "Product 1", Price = 100 }, new Product { Name = "Product 2", Price = 200 } }); });

// GET /products?name=mouse
// app.MapGet("/products", (string name) =>
// {
//     return Results.Ok($"Product {name}");
// });

// GET /products/1
// app.MapGet("/products/{id}", (int id) => { return Results.Ok($"Product {id}"); });

// GET /products/1
app.MapGet("/products/{id:int}", (int id) =>
{
    if (id == 1)
        return Results.Ok(new Product { Name = "Laptop", Price = 1000 });

    return Results.NotFound();
});

// app.MapGet("/products/{name}", (string name) => { return Results.Ok($"Product {name}"); });

// GET /products?name=mouse&price=100&dynamic=1&dynamic2=2
// app.MapGet("/products", (HttpContext context) =>
// {
//     var query = context.Request.Query;
//     var result = new StringBuilder();
//
//     foreach (var (key, value) in query)
//     {
//         result.AppendLine($"{key}: {value}");
//     }
//
//     return Results.Ok(result.ToString());
// });

// POST /products
app.MapPost("/products", (Product product) =>
{
    // Save product to the database
    return Results.Created("/products/1", product);
});

// PUT /products/1
app.MapPut("/products/{id}", (int id, Product product) =>
{
    // Update product in the database
    return Results.Ok(product);
});

// PATCH /products/1
app.MapPatch("/products/{id}", (int id, JsonElement patch) =>
{
    // Apply patch to the product in the database
    return Results.Ok(patch);
});

// DELETE /products/1
app.MapDelete("/products/{id}", (int id) =>
{
    // Delete product from the database

    if (id == 1)
        return Results.NoContent();

    return Results.NotFound();
});

#endregion

app.Run();


class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// need to set Program as public class
public partial class Program
{
}