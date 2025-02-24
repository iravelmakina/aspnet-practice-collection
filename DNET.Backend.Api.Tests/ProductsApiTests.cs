using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DNET.Backend.Api.Tests;

public sealed class ProductsApiTests : BaseApiTests
{
    public ProductsApiTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProducts_ShouldReturnAllProducts()
    {
        var response = await Client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct_WhenExists()
    {
        var response = await Client.GetAsync("/products/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal("Laptop", product.Name);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var response = await Client.GetAsync("/products/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreated()
    {
        var newProduct = new Product { Name = "Tablet", Price = 1000 };

        var response = await Client.PostAsJsonAsync("/products", newProduct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal("Tablet", product.Name);
    }
    
    [Fact]
    public async Task DeleteProduct_ShouldReturnNoContent_WhenProductExists()
    {
        var response = await Client.DeleteAsync("/products/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var response = await Client.DeleteAsync("/products/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}