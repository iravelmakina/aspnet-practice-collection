namespace DNET.Backend.DataAccess.Domain;

public class ClientEntity
{
    public int Id { get; set; }
    public Guid Uid { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string Phone { get; set; } = String.Empty;
    
    // Navigation property
    public ICollection<ReservationEntity> Reservations { get; set; } = [];
}