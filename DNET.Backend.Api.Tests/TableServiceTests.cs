using System.Text.Json;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using DNET.Backend.Api.Services;
using DNET.Backend.DataAccess;
using DNET.Backend.DataAccess.Domain;
using Microsoft.Extensions.Options;
using Moq;

namespace DNET.Backend.Api.Tests;


[Collection("TableServiceTests")]
public sealed class TableServiceTests
{
    private TableReservationsDbContext _dbContext;
    private TableService _tableService;
    
    public TableServiceTests()
    {
        var mockOptionsMonitor = new Mock<IOptionsMonitor<TableOptions>>();

        mockOptionsMonitor
            .Setup(o => o.CurrentValue)
            .Returns(new TableOptions { AllowTableCreation = true });

        _dbContext = Utils.CreateInMemoryDatabaseContext();
        _tableService = new TableService(_dbContext, mockOptionsMonitor.Object);
    }
    
    
    [Fact]
    public void TableService_ShouldCreateEmptyTablesList()
    {
        Assert.Empty(_dbContext.Tables.ToList());
    }
    
    
    [Fact]
    public void GetAllPaginatedTables_ShouldReturnAllPaginatedTables()
    {
        for (int i = 1; i <= 6; i++)
        {
            _dbContext.Tables.Add(new TableEntity
            {
                Id = i,
                Number = i,
                Capacity = i,
                LocationId = i
            });
        }
        _dbContext.SaveChanges();

        var result = _tableService.GetAllPaginatedTables(2, 3);

        Assert.NotNull(result);
        Assert.Equal(6, result.TotalItems);
        Assert.Equal(2, result.Page);
        Assert.Equal(3, result.Size);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(3, result.Tables.Count);
        Assert.Equal(4, result.Tables.First().Number);
        Assert.Equal(4, result.Tables.First().Capacity);
        Assert.Equal("Bar", result.Tables.First().Location);
        Assert.Equal(6, result.Tables.Last().Number);
        Assert.Equal(6, result.Tables.Last().Capacity);
        Assert.Equal("Garden", result.Tables.Last().Location);
    }
    
    
    [Fact]
    public void GetAllPaginatedTables_ShouldReturnNull_WhenDoesNotExist()
    {
        var result = _tableService.GetAllPaginatedTables(10, 5);
        
        Assert.Null(result);
    }
    
    
    [Fact]
    public void GetTable_ShouldReturnTable_WhenExists()
    {
        var newTable = new TableEntity { Number = 1, Capacity = 4, LocationId = 1 };
        _dbContext.Tables.Add(newTable);
        _dbContext.SaveChanges();
        
        var result = _tableService.GetTable(1);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.Number);
        Assert.Equal(4, result.Capacity);
        Assert.Equal("Main Hall", result.Location);
    }
    
    
    [Fact]
    public void GetTable_ShouldReturnNull_WhenDoesNotExist()
    {
        var result = _tableService.GetTable(777);
        
        Assert.Null(result);
    }
    
    
    [Fact]
    public void GetTablesByCapacity_ShouldReturnAllFilteredTables_WhenExist()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1 });
        _dbContext.Tables.Add(new TableEntity { Number = 2, Capacity = 6, LocationId = 2 });
        _dbContext.Tables.Add(new TableEntity { Number = 3, Capacity = 4, LocationId = 3 });
        _dbContext.SaveChanges();
        
        var result = _tableService.GetTablesByCapacity(4);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Number);
        Assert.Equal(3, result[1].Number);
        Assert.Equal("Main Hall", result[0].Location);
        Assert.Equal("Private Room", result[1].Location);
    }
    
    
    [Fact]
    public void GetTablesByCapacity_ShouldReturnNull_WhenDoesNotExist()
    {
        var result = _tableService.GetTablesByCapacity(777);
        
        Assert.Null(result);
    }
    
    
    [Fact]
    public void CreateTable_ShouldReturnCreatedTuple_WhenAllowed()
    {
        var newTable = new Table { Number = 1, Capacity = 4, Location = "Main Hall" };

        var result = _tableService.CreateTable(newTable);
        
         _dbContext.SaveChanges();

        Assert.NotNull(result);
        Assert.Equal(1, result.Item1);
        Assert.Equal(newTable, result.Item2);
    }
    
    
    [Fact]
    public void CreateTable_ShouldReturnNull_WhenCreationIsDisabled()
    {
        var options = new Mock<IOptionsMonitor<TableOptions>>();
        options.Setup(o => o.CurrentValue).Returns(new TableOptions { AllowTableCreation = false });
        
        var disabledService = new TableService(_dbContext, options.Object);

        var newTable = new Table { Number = 1, Capacity = 4, Location = "Main Hall" };
    
        var result = disabledService.CreateTable(newTable);
    
        Assert.Null(result);
    }
    
    
    [Fact]
    public void CreateTable_ShouldThrowException_WhenTableWithSameNumberAlreadyExists()
    {
       
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1 });
        _dbContext.SaveChanges();
    
        var newTable = new Table { Number = 1, Capacity = 6, Location = "Patio" };
    
        Assert.Throws<ServerException>(() => _tableService.CreateTable(newTable));
    }
    
    
    [Fact]
    public void CreateTable_ShouldThrowException_WhenLocationDoesNotExist()
    {
        var newTable = new Table { Number = 1, Capacity = 4, Location = "Swimming Pool" };
    
        Assert.Throws<ServerException>(() => _tableService.CreateTable(newTable));
    }
    
    
    [Fact]
    public void UpdateTable_ShouldReturnUpdatedTable_WhenExists()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1 });
        _dbContext.SaveChanges();
    
        var updatedTable = new Table { Number = 1, Capacity = 6, Location = "Patio" };
    
    
        var result = _tableService.UpdateTable(1, updatedTable);
    
        Assert.NotNull(result);
        Assert.Equal(1, result.Number);
        Assert.Equal(6, result.Capacity);
        Assert.Equal("Patio", result.Location);
    }
    
    
    [Fact]
    public void UpdateTable_ShouldReturnNull_WhenTableDoesNotExist()
    {
        var updatedTable = new Table { Number = 1, Capacity = 6, Location = "Main Hall" };
    
        var result = _tableService.UpdateTable(1, updatedTable);
    
        Assert.Null(result);
    }
    
    
    [Fact]
    public void UpdateTable_ShouldThrowException_WhenTableWithSameNumberAlreadyExists()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1 });
        _dbContext.Tables.Add(new TableEntity { Number = 2, Capacity = 6, LocationId = 2 });
        _dbContext.SaveChanges();
        
        var updatedTable = new Table { Number = 2, Capacity = 8, Location = "Private Room" };
    
        Assert.Throws<ServerException>(() => _tableService.UpdateTable(1, updatedTable));
    }
    
    
    [Fact]
    public void UpdateTable_ShouldThrowException_WhenLocationDoesNotExist()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1 });
        _dbContext.SaveChanges();
    
        var updatedTable = new Table { Number = 1, Capacity = 6, Location = "Swimming Pool" };
    
        Assert.Throws<ServerException>(() => _tableService.UpdateTable(1, updatedTable));
    }
    
    
    [Fact]
    public void PatchTable_ShouldReturnUpdatedTable_WhenValidPatch()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1 });
        _dbContext.SaveChanges();

        var patchJson = """{ "capacity": 6 }""";
        var patch = JsonDocument.Parse(patchJson).RootElement;

        var result = _tableService.PatchTable(1, patch);

        Assert.NotNull(result);
        Assert.Equal(1, result.Number);
        Assert.Equal(6, result.Capacity);
        Assert.Equal("Main Hall", result.Location);
    }
    
    
    [Fact]
    public void PatchTable_ShouldReturnNull_WhenTableDoesNotExist()
    {
        var patchJson = """{ "capacity": 6 }""";
        var patch = JsonDocument.Parse(patchJson).RootElement;

        var result = _tableService.PatchTable(1, patch);

        Assert.Null(result);
    }
    
    
    [Fact]
    public void PatchTable_ShouldThrowException_WhenTableWithSameNumberAlreadyExists()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1 });
        _dbContext.Tables.Add(new TableEntity { Number = 2, Capacity = 6, LocationId = 2 });
        _dbContext.SaveChanges();

        var patchJson = """{ "number": 2 }""";
        var patch = JsonDocument.Parse(patchJson).RootElement;

        Assert.Throws<ServerException>(() => _tableService.PatchTable(1, patch));
    }
    
    
    [Fact]
    public void PatchTable_ShouldThrowException_WhenLocationDoesNotExist()
    {
        _dbContext.Tables.Add(new TableEntity { Number = 1, Capacity = 4, LocationId = 1 });
        _dbContext.SaveChanges();

        var patchJson = """{ "location": "Swimming Pool" }""";
        var patch = JsonDocument.Parse(patchJson).RootElement;

        Assert.Throws<ServerException>(() => _tableService.PatchTable(1, patch));
    }
    
    
    [Fact]
    public void DeleteTable_ShouldReturnTrue_WhenTableExists()
    {
        var tableToDelete = new TableEntity { Number = 1, Capacity = 4, LocationId = 1 };
        _dbContext.Tables.Add(tableToDelete);
        _dbContext.SaveChanges();

        var result = _tableService.DeleteTable(1);

        Assert.True(result);
        Assert.Null(_dbContext.Tables.Find(1));
    }

    [Fact]
    public void DeleteTable_ShouldReturnFalse_WhenTableDoesNotExist()
    {
        var result = _tableService.DeleteTable(777);

        Assert.False(result);
    }
}
