namespace DNET.Backend.Api.Models;

public class Reservation
{
    public Reservation(int clientId, int tableId, string startTime, string endTime)
    {
        ClientId = clientId;
        TableId = tableId;
        StartTime = DateTime.Parse(startTime);
        EndTime = DateTime.Parse(endTime);
    }
    
    public int ClientId { get; set; }
    public int TableId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
