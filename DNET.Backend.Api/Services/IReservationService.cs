using DNET.Backend.Api.Models;
using DNET.Backend.Api.Requests;

namespace DNET.Backend.Api.Services;

public interface IReservationService
{

    public ReservationDTO? GetReservation(int id);
    
    public List<ReservationDTO> GetAllReservations(int? clientId, int? tableNumber, DateTime? date, String? reservationType);

    public Tuple<int, ReservationDTO>? AddReservation(CreateUpdateReservationRequest request);
    
    public ReservationDTO? UpdateReservation(int id, CreateUpdateReservationRequest request);
    
    public bool DeleteReservation(int id);

}