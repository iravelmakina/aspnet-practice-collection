using System.Text.Json;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using DNET.Backend.Api.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace DNET.Backend.Api.Tests;

[Collection("TableServiceTests")]
public sealed class TablesServiceTest
{
    private static TableService ConfigureTableService(bool allowCreation = true)
    {
        var mockOptionsMonitor = new Mock<IOptionsMonitor<TableOptions>>();

        mockOptionsMonitor
            .Setup(o => o.CurrentValue)
            .Returns(new TableOptions { AllowTableCreation = allowCreation });

        return new TableService(mockOptionsMonitor.Object);
    }
    
    
    [Fact]
    public void TableService_ShouldCreateEmptyTablesList()
    {
        var tableService = ConfigureTableService();
        Assert.Empty(tableService.Tables);
    }
    
    
    [Fact]
    public void GetAllPaginatedTables_ShouldReturnAllPaginatedTables()
    {
        var tableService = ConfigureTableService();

        for (int i = 1; i <= 15; i++)
        {
            var table = new Table { Id = i, Capacity = i };
            tableService.Tables.Add(i, table);
        }

        var result = tableService.GetAllPaginatedTables(2, 5);

        Assert.NotNull(result);
        Assert.Equal(15, result.Item1);
        Assert.Equal(2, result.Item2);
        Assert.Equal(5, result.Item3);
        Assert.Equal(3, result.Item4);
        Assert.Equal(5, result.Item5.Count);
        Assert.Equal(6, result.Item5.First().Id);
        Assert.Equal(6, result.Item5.First().Capacity);
        Assert.Equal(10, result.Item5.Last().Id);
        Assert.Equal(10, result.Item5.Last().Capacity);
    }
    
    
    [Fact]
    public void GetAllPaginatedTables_ShouldReturnNull_WhenDoesNotExist()
    {
        var tableService = ConfigureTableService();
        var result = tableService.GetAllPaginatedTables(10, 5);
        
        Assert.Null(result);
    }
    
    
    [Fact]
    public void GetTable_ShouldReturnTable_WhenExists()
    {
        var tableService = ConfigureTableService();
        tableService.Tables = new Dictionary<int, Table>
        {
            { 1, new Table { Id = 1, Capacity = 4 } }
        };
        
        var result = tableService.GetTable(1);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(4, result.Capacity);
    }
    
    
    [Fact]
    public void GetTable_ShouldReturnNull_WhenDoesNotExist()
    {
        var tableService = ConfigureTableService();
        var result = tableService.GetTable(777);
        
        Assert.Null(result);
    }
    
    
    [Fact]
    public void GetTablesByCapacity_ShouldReturnAllFilteredTables_WhenExist()
    {
        var tableService = ConfigureTableService();
        tableService.Tables = new Dictionary<int, Table>
        {
            { 1, new Table { Id = 1, Capacity = 4 } },
            { 2, new Table { Id = 2, Capacity = 6 } },
            { 3, new Table { Id = 3, Capacity = 4 } }
        };
        
        var result = tableService.GetTablesByCapacity(4);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(3, result[1].Id);
    }
    
    
    [Fact]
    public void GetTablesByCapacity_ShouldReturnNull_WhenDoesNotExist()
    {
        var tableService = ConfigureTableService();
        var result = tableService.GetTablesByCapacity(777);
        
        Assert.Null(result);
    }
    
    
    [Fact]
    public void CreateTable_ShouldReturnCreatedTuple_WhenAllowed()
    {
        var tableService = ConfigureTableService();
        var newTable = new Table { Id = 1, Capacity = 4 };

        var result = tableService.CreateTable(newTable);

        Assert.NotNull(result);
        Assert.Equal(1, result.Item1);
        Assert.Equal(newTable, result.Item2);
    }
    
    
    [Fact]
    public void CreateTable_ShouldReturnNull_WhenCreationIsDisabled()
    {
        var tableService = ConfigureTableService(allowCreation: false);
        var newTable = new Table { Id = 1, Capacity = 4 };

        var result = tableService.CreateTable(newTable);

        Assert.Null(result);
    }
    
    
    [Fact]
    public void CreateTable_ShouldThrowException_WhenTableWithSameIdAlreadyExists()
    {
        var tableService = ConfigureTableService();
        tableService.Tables = new Dictionary<int, Table>
        {
            { 1, new Table { Id = 1, Capacity = 4 } }
        };

        var newTable = new Table { Id = 1, Capacity = 6 };

        Assert.Throws<BadRequestException>(() => tableService.CreateTable(newTable));
    }
    
    
    [Fact]
    public void UpdateTable_ShouldReturnUpdatedTable_WhenExists()
    {
        var tableService = ConfigureTableService();
        tableService.Tables = new Dictionary<int, Table>
        {
            { 1, new Table { Id = 1, Capacity = 4 } }
        };

        var updatedTable = new Table { Id = 1, Capacity = 6 };

        var result = tableService.UpdateTable(1, updatedTable);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(6, result.Capacity);
    }
    
    
    [Fact]
    public void UpdateTable_ShouldReturnNull_WhenTableDoesNotExist()
    {
        var tableService = ConfigureTableService();
        var updatedTable = new Table { Id = 1, Capacity = 6 };

        var result = tableService.UpdateTable(1, updatedTable);

        Assert.Null(result);
    }
    
    
    [Fact]
    public void UpdateTable_ShouldThrowException_WhenTableWithSameIdAlreadyExists()
    {
        var tableService = ConfigureTableService();
        tableService.Tables = new Dictionary<int, Table>
        {
            { 1, new Table { Id = 1, Capacity = 4 } },
            { 2, new Table { Id = 2, Capacity = 6 } }
        };

        var updatedTable = new Table { Id = 2, Capacity = 8 };

        Assert.Throws<BadRequestException>(() => tableService.UpdateTable(1, updatedTable));
    }
    
    
    [Fact]
    public void PatchTable_ShouldReturnUpdatedTable_WhenValidPatch()
    {
        var tableService = ConfigureTableService();
        tableService.Tables = new Dictionary<int, Table>
        {
            { 1, new Table { Id = 1, Capacity = 4 } }
        };

        var patchJson = """{ "capacity": 6 }""";
        var patch = JsonDocument.Parse(patchJson).RootElement;

        var result = tableService.PatchTable(1, patch);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(6, result.Capacity);
    }
    
    
    [Fact]
    public void PatchTable_ShouldReturnNull_WhenTableDoesNotExist()
    {
        var tableService = ConfigureTableService();

        var patchJson = """{ "capacity": 6 }""";
        var patch = JsonDocument.Parse(patchJson).RootElement;

        var result = tableService.PatchTable(1, patch);

        Assert.Null(result);
    }
    
    
    [Fact]
    public void PatchTable_ShouldThrowException_WhenTableWithSameIdAlreadyExists()
    {
        var tableService = ConfigureTableService();
        tableService.Tables = new Dictionary<int, Table>
        {
            { 1, new Table { Id = 1, Capacity = 4 } },
            { 2, new Table { Id = 2, Capacity = 6 } }
        };

        var patchJson = """{ "id": 2 }""";
        var patch = JsonDocument.Parse(patchJson).RootElement;

        Assert.Throws<BadRequestException>(() => tableService.PatchTable(1, patch));
    }
    
    
    [Fact]
    public void DeleteTable_ShouldReturnTrue_WhenTableExists()
    {
        var tableService = ConfigureTableService();
        tableService.Tables = new Dictionary<int, Table>
        {
            { 1, new Table { Id = 1, Capacity = 4 } }
        };

        var result = tableService.DeleteTable(1);

        Assert.True(result);
        Assert.Empty(tableService.Tables);
    }

    [Fact]
    public void DeleteTable_ShouldReturnFalse_WhenTableDoesNotExist()
    {
        var tableService = ConfigureTableService();

        var result = tableService.DeleteTable(777);

        Assert.False(result);
    }
}
