using DNET.Backend.DataAccess.Domain;

namespace DNET.Backend.Api.Models;

public class Table
{
    public int Number { get; set; }
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;
    
    public Table() { }
    
    public Table(TableEntity table)
    {
        Number = table.Number;
        Capacity = table.Capacity;
        Location = table.Location?.Name ?? string.Empty;
    }
}
