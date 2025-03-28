namespace DNET.Backend.DataAccess.Domain;

public class HostEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TableId { get; set; } // foreign key

    // Navigation property to TableEntity
    public TableEntity Table { get; set; } = null!; // one Host is responsible for one Table
}
