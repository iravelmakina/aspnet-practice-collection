using DNET.Backend.Api.Models;
using DNET.Backend.Api.Requests;

namespace DNET.Backend.Api.Services;

public interface IReservationService
{

    public Reservation? GetReservation(int id);
    
    public List<Reservation> GetAllReservations(int? clientId, int? tableNumber, DateTime? date, String? reservationType);

    public Tuple<int, Reservation>? AddReservation(CreateUpdateReservationRequest request);
    
    public Reservation? UpdateReservation(int id, CreateUpdateReservationRequest request);
    
    public bool DeleteReservation(int id);

}