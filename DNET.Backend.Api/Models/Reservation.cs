namespace DNET.Backend.Api.Models;

public class Reservation
{
    public int ClientId { get; set; }
    public int TableId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
