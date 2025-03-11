using System.Text.Json;
using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Services;

public interface ITableService
{
    public Tuple<int, int, int, int, List<Table>>? GetAllPaginatedTables(int page = 1, int size = 10);
    
    public Table? GetTable(int id);
    
    public List<Table>? GetTablesByCapacity(int capacity);
    
    /// <exception cref="BadRequestException"></exception>
    public Tuple<int, Table>? CreateTable(Table table);
    
    /// <exception cref="BadRequestException"></exception>
    public Table? UpdateTable(int id, Table table);
    
    /// <exception cref="BadRequestException"></exception>
    public Table? PatchTable(int id, JsonElement patch);
    
    public bool DeleteTable(int id);
}