using System.Text.Json;
using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Services;

public interface ITableService
{
    public GetTablesResponse? GetAllPaginatedTables(int page = 1, int size = 10);
    
    public Table? GetTable(int id);
    
    public List<Table>? GetTablesByCapacity(int capacity);
    
    /// <exception cref="ServerException"></exception>
    public Tuple<int, Table>? CreateTable(Table table);
    
    /// <exception cref="ServerException"></exception>
    public Table? UpdateTable(int id, Table table);
    
    /// <exception cref="ServerException"></exception>
    public Table? PatchTable(int id, JsonElement patch);
    
    public bool DeleteTable(int id);
}