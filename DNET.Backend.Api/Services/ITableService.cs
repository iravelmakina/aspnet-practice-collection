using System.Text.Json;
using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Services;

public interface ITableService
{
    public Tuple<int, int, int, int, List<TableDTO>>? GetAllPaginatedTables(int page = 1, int size = 10);
    
    public TableDTO? GetTable(int id);
    
    public List<TableDTO>? GetTablesByCapacity(int capacity);
    
    /// <exception cref="BadRequestException"></exception>
    public Tuple<int, TableDTO>? CreateTable(TableDTO table);
    
    /// <exception cref="BadRequestException"></exception>
    public TableDTO? UpdateTable(int id, TableDTO table);
    
    /// <exception cref="BadRequestException"></exception>
    public TableDTO? PatchTable(int id, JsonElement patch);
    
    public bool DeleteTable(int id);
}