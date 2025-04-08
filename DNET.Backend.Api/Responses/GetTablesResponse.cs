namespace DNET.Backend.Api.Models;

public class GetTablesResponse
{
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    public List<Table> Tables { get; set; } = new();
}
