using DNET.Backend.DataAccess.Domain;

namespace DNET.Backend.Api.Models;

public class TableDTO
{
    public int Number { get; set; }
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;
    
    public TableDTO() { }
    
    public TableDTO(TableEntity table)
    {
        Number = table.Number;
        Capacity = table.Capacity;
        Location = table.Location?.Name ?? string.Empty;
    }
}
