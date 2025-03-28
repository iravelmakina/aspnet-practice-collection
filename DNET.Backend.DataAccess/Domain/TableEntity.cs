namespace DNET.Backend.DataAccess.Domain;

public class TableEntity
{
    public int Id { get; set; }
    public int Number { get; set; } // table number (different from Id in db table)
    public int Capacity { get; set; }
    public int LocationId { get; set; }  // foreign key
     
    // Navigation property
    public LocationEntity Location { get; set; } = null!; // one Table has one Location
    public HostEntity Host { get; set; } = null!; // one Table has one Host
    public List<ReservationEntity> Reservations { get; set; } = [];

}
