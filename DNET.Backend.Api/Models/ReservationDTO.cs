using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using DNET.Backend.DataAccess.Domain;

namespace DNET.Backend.Api.Models;

public class ReservationDTO
{
    public ReservationDTO() { }
    
    public ReservationDTO(ReservationEntity reservation)
    {
        TableNumber = reservation.Table.Number;
        StartTime = reservation.StartTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        EndTime = reservation.EndTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        ClientName = reservation.Client.Name;
        ClientPhone = reservation.Client.Phone;
        HostName = reservation.Table.Host?.Name;
        ReservationType = reservation.ReservationDetail.ReservationType.ToString();
        SpecialRequests = reservation.ReservationDetail.SpecialRequests;
    }
    
    public int TableNumber { get; set; }
    public String StartTime { get; set; }
    public String EndTime { get; set; }
    public String ClientName { get; set; } = String.Empty;
    public String ClientPhone { get; set; } = String.Empty;
    public String? HostName { get; set; }
    public String ReservationType { get; set; } = String.Empty;
    public String SpecialRequests { get; set; } = String.Empty;
}