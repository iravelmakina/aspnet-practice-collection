using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Services;

public interface IReservationService
{

    public Reservation? GetReservation(int id);
    
    public List<Reservation>? GetAllReservations(int? tableId, DateTime? date);

    public Tuple<int, Reservation> AddReservation(Reservation reservation);
    
    public Reservation? UpdateReservation(int id, Reservation reservation);
    
    public bool DeleteReservation(int id);

}