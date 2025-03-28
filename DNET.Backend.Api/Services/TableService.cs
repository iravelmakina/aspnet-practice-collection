using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;
using DNET.Backend.DataAccess;
using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;

namespace DNET.Backend.Api.Services;

public class TableService : ITableService
{
    private readonly TableReservationsDbContext _dbContext;
    private readonly IOptionsMonitor<TableOptions> _optionsDelegate;
    
    public TableService(TableReservationsDbContext dbContext, IOptionsMonitor<TableOptions> optionsDelegate)
    {
        _dbContext = dbContext;
        _optionsDelegate = optionsDelegate;
    }
    
    public Tuple<int, int, int, int, List<TableDTO>>? GetAllPaginatedTables(int page = 1, int size = 10)
    {
        var totalItems = _dbContext.Tables.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
            
        var paginatedTables = _dbContext.Tables
            .Include(t => t.Location)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(t => new TableDTO(t))
            .ToList();
        
        if (paginatedTables.Count == 0)
            return null;
        
        return new Tuple<int, int, int, int, List<TableDTO>>(totalItems, page, size, totalPages, paginatedTables);
    }


    public TableDTO? GetTable(int id)
    {
        var table = _dbContext.Tables
            .Include(t => t.Location)
            .FirstOrDefault(t => t.Id == id);

        if (table == null)
            return null;

        return new TableDTO(table);
    }

    public List<TableDTO>? GetTablesByCapacity(int capacity)
    {
        var tables = _dbContext.Tables
            .Include(t => t.Location)
            .Where(x => x.Capacity == capacity)
            .Select(t => new TableDTO(t))
            .ToList();
        
        if (tables.Count == 0)
            return null;
        
        return tables;
    }

    
    public Tuple<int, TableDTO>? CreateTable(TableDTO table)
    {
        if (!_optionsDelegate.CurrentValue.AllowTableCreation)
            return null; // Feature disabled
        
        if (_dbContext.Tables.Any(t => t.Number == table.Number))
            throw new BadRequestException("A table with this number already exists.", 400);
        
        var location = _dbContext.Locations
            .FirstOrDefault(l => l.Name == table.Location);
    
        if (location == null)
            throw new BadRequestException("The specified location does not exist.", 400);
        
        var tableEntity = new TableEntity
        {
            Number = table.Number,
            Capacity = table.Capacity,
            LocationId = location.Id
        };
        
        _dbContext.Tables.Add(tableEntity);
        
        _dbContext.SaveChanges();
        
        return new Tuple<int, TableDTO>(tableEntity.Id, table);
    }

    
    public TableDTO? UpdateTable(int id, TableDTO table)
    {
        var existingTable = _dbContext.Tables
            .Include(t => t.Location)
            .FirstOrDefault(t => t.Id == id);
        
        if (existingTable == null)
            return null;
        
        if (table.Number != existingTable.Number && _dbContext.Tables.Any(t => t.Number == table.Number))
            throw new BadRequestException("A table with this number already exists.", 400);
        
        var location = _dbContext.Locations
            .FirstOrDefault(l => l.Name == table.Location);
        
        if (location == null)
            throw new BadRequestException("The specified location does not exist.", 400);
        
        existingTable.Number = table.Number;
        existingTable.Capacity = table.Capacity;
        existingTable.Location.Name = table.Location;
        
        return new TableDTO(existingTable);
    }
    
    
    public TableDTO? PatchTable(int id, JsonElement patch)
    {
        var existingTable = _dbContext.Tables
            .Include(t => t.Location)
            .FirstOrDefault(t => t.Id == id);
    
        if (existingTable == null)
            return null;
        
        if (patch.TryGetProperty("number", out var numberElement))
        {
            var newNumber = numberElement.GetInt32();
            if (newNumber != existingTable.Number && _dbContext.Tables.Any(t => t.Number == newNumber))
                throw new BadRequestException("A table with this number already exists.", 400);
            
            existingTable.Number = newNumber;
        }
        
        if (patch.TryGetProperty("capacity", out var capacityElement))
            existingTable.Capacity = capacityElement.GetInt32();
        
        if (patch.TryGetProperty("location", out var locationElement))
        {
            var newLocation = locationElement.GetString();
            var location = _dbContext.Locations
                .FirstOrDefault(l => l.Name == newLocation);
            
            if (location == null)
                throw new BadRequestException("The specified location does not exist.", 400);
            
            existingTable.Location.Id = location.Id;
        }
        
        _dbContext.SaveChanges();
        
        return new TableDTO(existingTable);
    }

    
    public bool DeleteTable(int id)
    {
        var table = _dbContext.Tables.Find(id);

        if (table == null)
            return false;

        _dbContext.Tables.Remove(table);
        _dbContext.SaveChanges();

        return true;
    }
}
