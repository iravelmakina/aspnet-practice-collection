using System.Text.Json;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Requests;
using Microsoft.Extensions.Caching.Memory;

namespace DNET.Backend.Api.Services;

public class TableServiceWithCache : ITableService
{
    private readonly ITableService _tableService;
    private readonly IMemoryCache _memoryCache;

    public TableServiceWithCache(ITableService tableService, IMemoryCache memoryCache)
    {
        _tableService = tableService;
        _memoryCache = memoryCache;
    }

    public GetTablesResponse? GetAllPaginatedTables(int page = 1, int size = 10)
    {
        var cacheKey = $"tables_{page}_{size}";
        
        if (_memoryCache.TryGetValue(cacheKey, out GetTablesResponse? tables))
        {
            return tables;
        }
        
        tables = _tableService.GetAllPaginatedTables(page, size);
        if (tables != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            _memoryCache.Set(cacheKey, tables, cacheOptions);
        }
        
        return tables;
    }

    public Table? GetTable(int id)
    {
        var cacheKey = $"table_{id}";
        
        if (_memoryCache.TryGetValue(cacheKey, out Table? table))
        {
            return table;
        }
        
        table = _tableService.GetTable(id);
        if (table != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            _memoryCache.Set(cacheKey, table, cacheOptions);
        }
        
        return table;
    }

    public List<Table>? GetTablesByCapacity(int capacity)
    {
        var cacheKey = $"tables_capacity_{capacity}";
        
        if (_memoryCache.TryGetValue(cacheKey, out List<Table>? tables))
        {
            return tables;
        }
        
        tables = _tableService.GetTablesByCapacity(capacity);
        if (tables != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            _memoryCache.Set(cacheKey, tables, cacheOptions);
        }
        
        return tables;
    }

    public Tuple<int, Table>? CreateTable(Table table) => _tableService.CreateTable(table);
    public Table? UpdateTable(int id, Table table) => _tableService.UpdateTable(id, table);
    public Table? PatchTable(int id, JsonElement patch) => _tableService.PatchTable(id, patch);
    public bool DeleteTable(int id) => _tableService.DeleteTable(id);
}