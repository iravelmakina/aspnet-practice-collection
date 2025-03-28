namespace DNET.Backend.DataAccess.Domain;

public class ReservationEntity
{
    public int Id { get; set; }
    
    public Guid Uid { get; set; }
    
    public int ClientId { get; set; }  // Foreign key
    
    public int TableId { get; set; }  // Foreign key
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }

    // Navigation property
    public ReservationDetailEntity ReservationDetail { get; set; } = null!;
    public ClientEntity Client { get; set; } = null!; // one client to many reservations
    public TableEntity Table { get; set; } = null!;  // one table to many reservations
}
