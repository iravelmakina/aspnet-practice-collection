namespace DNET.Backend.Api.Requests;

public class CreateUpdateReservationRequest
{
    public int ClientId { get; set; }
    public int TableNumber { get; set; }
    public String StartTime { get; set; }
    public String EndTime { get; set; }
    public String ReservationType { get; set; }
    public String SpecialRequests { get; set; }
}
