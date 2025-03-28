namespace DNET.Backend.DataAccess.Domain;

public class LocationEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Navigation property
    public List<TableEntity> Tables { get; set; } = []; // one Location has many Tables
}