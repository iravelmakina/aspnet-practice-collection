using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Tests;


[Collection("TablesApiTests")]
public sealed class TablesApiTests : BaseApiTests
{
    public TablesApiTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }


    [Fact]
    public async Task GetTablesByPageAndSize_ShouldReturnPaginatedTables()
    {
        for (int i = 1; i <= 15; i++)
        {
            await Client.PostAsync("/tables", new StringContent(
                $$"""{"Id":{{i}},"Capacity":{{i}}}""",
                Encoding.UTF8,
                "application/json"
            ));
        }

        var response = await Client.GetAsync("/tables?page=2&size=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var expectedJson =
            """{"totalItems":15,"page":2,"pageSize":5,"totalPages":3,"items":[{"id":6,"capacity":6},{"id":7,"capacity":7},{"id":8,"capacity":8},{"id":9,"capacity":9},{"id":10,"capacity":10}]}""";
        Assert.Equal(expectedJson, json);

        for (int i = 1; i <= 15; i++)
            await Client.DeleteAsync($"/tables/{i}");
    }


    [Fact]
    public async Task GetTablesByPageAndSize_ShouldReturnNotFound_WhenDoesNotExist()
    {
        var response = await Client.GetAsync("/tables?page=10&size=5");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task GetTableById_ShouldReturnTable_WhenExists()
    {
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":1,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));

        var response = await Client.GetAsync("/tables/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal("""{"id":1,"capacity":4}""", json);

        await Client.DeleteAsync("/tables/1");
    }


    [Fact]
    public async Task GetTableById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        var response = await Client.GetAsync("/products/777");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task GetTablesByCapacity_ShouldReturnAllFilteredTables()
    {
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":1,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":2,"Capacity":6}""",
            Encoding.UTF8,
            "application/json"
        ));
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":3,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));
    
        var response = await Client.GetAsync("/tables/filter?capacity=4");
    
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal("""[{"id":1,"capacity":4},{"id":3,"capacity":4}]""", json);
    
        await Client.DeleteAsync("/tables/1");
        await Client.DeleteAsync("/tables/2");
        await Client.DeleteAsync("/tables/3");
    }


    [Fact]
    public async Task GetTablesByCapacity_ShouldReturnNotFound_WhenTableDoesNotExist()
    {
        var response = await Client.GetAsync("/tables/filter?capacity=777");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task CreateTable_ShouldReturnCreated()
    {
        var newTable = new Table { Id = 1, Capacity = 4 };

        var response = await Client.PostAsJsonAsync("/tables", newTable);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal("""{"id":1,"capacity":4}""", json);

        await Client.DeleteAsync("/tables/1");
    }
    
    
    [Fact]
    public async Task CreateTable_ShouldReturnBadRequest_WhenTableWithSameIdAlreadyExists()
    {
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":1,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));

        var newTable = new Table { Id = 1, Capacity = 4 };

        var response = await Client.PostAsJsonAsync("/tables", newTable);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await Client.DeleteAsync("/tables/1");
    }


    [Fact]
    public async Task UpdateTable_ShouldReturnUpdated()
    {
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":1,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));

        var updatedTable = new Table { Id = 2, Capacity = 6 };

        var response = await Client.PutAsJsonAsync("/tables/1", updatedTable);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal("""{"id":2,"capacity":6}""", json);

        await Client.DeleteAsync("/tables/1");
    }


    [Fact]
    public async Task UpdateTable_ShouldReturnBadRequest_WhenTableWithSameIdAlreadyExists()
    {
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":1,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":2,"Capacity":6}""",
            Encoding.UTF8,
            "application/json"
        ));

        var updatedTable = new Table { Id = 2, Capacity = 6 };

        var response = await Client.PutAsJsonAsync("/tables/1", updatedTable);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await Client.DeleteAsync("/tables/1");
        await Client.DeleteAsync("/tables/2");
    }
    
    
    [Fact]
    public async Task UpdateTable_ShouldReturnNotFound_WhenTableDoesNotExist()
    {
        var updatedTable = new Table { Id = 1, Capacity = 6 };

        var response = await Client.PutAsJsonAsync("/tables/777", updatedTable);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    
    [Fact]
    public async Task PatchTable_ShouldReturnPatched()
    {
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":1,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));
    
        var patch = new { Capacity = 6 };
    
        var response = await Client.PatchAsJsonAsync("/tables/1", patch);
    
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal("""{"id":1,"capacity":6}""", json);
    
        await Client.DeleteAsync("/tables/1");
    }


    [Fact]
    public async Task PatchTable_ShouldReturnNotFound_WhenTableDoesNotExist()
    {
        var patch = new { Id = 1, Capacity = 6 };

        var response = await Client.PatchAsJsonAsync("/tables/777", patch);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    
    [Fact]
    public async Task PatchTable_ShouldReturnBadRequest_WhenTableWithSameIdAlreadyExists()
    {
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":1,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":2,"Capacity":6}""",
            Encoding.UTF8,
            "application/json"
        ));

        var patch = new { Id = 2 };

        var response = await Client.PatchAsJsonAsync("/tables/1", patch);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await Client.DeleteAsync("/tables/1");
        await Client.DeleteAsync("/tables/2");
    }


    [Fact]
    public async Task DeleteTable_ShouldReturnNoContent_WhenProductExists()
    {
        await Client.PostAsync("/tables", new StringContent(
            """{"Id":1,"Capacity":4}""",
            Encoding.UTF8,
            "application/json"
        ));

        var response = await Client.DeleteAsync("/tables/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }


    [Fact]
    public async Task DeleteTable_ShouldReturnNotFound_WhenTableDoesNotExist()
    {
        var response = await Client.DeleteAsync("/tables/777");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
