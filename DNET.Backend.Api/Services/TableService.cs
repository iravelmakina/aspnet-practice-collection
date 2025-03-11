using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DNET.Backend.Api.Services;

public class TableService : ITableService
{
    public Dictionary <int, Table> Tables  { get; set; }
    private readonly IOptionsMonitor<TableOptions> _optionsDelegate;
    
    public TableService(IOptionsMonitor<TableOptions> optionsDelegate)
    {
        Tables = new Dictionary<int, Table>();
        _optionsDelegate = optionsDelegate;
    }
    
    public Tuple<int, int, int, int, List<Table>>? GetAllPaginatedTables(int page = 1, int size = 10)
    {
        var totalItems = Tables.Values.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / size);
            
        var paginatedTables = Tables.Values
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();
        
        if (paginatedTables.Count == 0)
            return null; // Not Found
        
        return new Tuple<int, int, int, int, List<Table>>(totalItems, page, size, totalPages, paginatedTables);
    }

    
    public Table? GetTable(int id)
    {
        if (!Tables.TryGetValue(id, out var table))
            return null; // Not Found
        
        return table;
    } //coolcoolcool

    
    public List<Table>? GetTablesByCapacity(int capacity)
    {
        var tables = Tables.Values
            .Where(x => x.Capacity == capacity)
            .ToList();
        
        if (tables.Count == 0)
            return null; // Not Found
        
        return tables;
    }

    
    public Tuple<int, Table>? CreateTable(Table table)
    {
        if (!_optionsDelegate.CurrentValue.AllowTableCreation)
            return null; // Feature disabled
        
        if (Tables.ContainsKey(table.Id)) 
            throw new BadRequestException("A table with this ID already exists.", 400); // BadRequest
        // BadRequest
        
        int newId = Tables.Count + 1;
        Tables[newId] = table;
        
        return new Tuple<int, Table>(newId, table);
    }

    
    public Table? UpdateTable(int id, Table table)
    {
        if (!Tables.TryGetValue(id, out var existingTable))
            return null; // Not Found
        
        if (table.Id != id && Tables.ContainsKey(table.Id))
            throw new BadRequestException("A table with this ID already exists.", 400); // BadRequest
        
        existingTable.Id = table.Id;
        existingTable.Capacity = table.Capacity;
        
        return existingTable;
    }
    
    
    public Table? PatchTable(int id, JsonElement patch)
    {
        if (!Tables.TryGetValue(id, out var existingTable))
            return null; // Not Found
        
        if (patch.TryGetProperty("id", out var idElement))
        {
            var newId = idElement.GetInt32();
            if (newId != id && Tables.ContainsKey(newId))
                throw new BadRequestException("A table with this ID already exists.", 400); // BadRequest
            
            existingTable.Id = newId;
        }
        
        if (patch.TryGetProperty("capacity", out var capacityElement))
            existingTable.Capacity = capacityElement.GetInt32();
        
        return existingTable;
    }

    
    public bool DeleteTable(int id)
    {
        return Tables.Remove(id); // Not Found
    }
}