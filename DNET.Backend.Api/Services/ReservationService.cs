using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Services;

public class ReservationService : IReservationService
{
    private Dictionary<int, Reservation> Reservations  { get; set; }

    public ReservationService()
    {
        Reservations = new Dictionary<int, Reservation>();
    }

    public Reservation? GetReservation(int id)
    {
        if (!Reservations.TryGetValue(id, out var reservation))
            return null;
    
        return reservation;
    }
    
    public List<Reservation> GetAllReservations(int? tableId = null, DateTime? date = null)
    {
        var filtered = Reservations.AsEnumerable();

        if (tableId.HasValue)
            filtered = Reservations.Where(x => x.Value.TableId == tableId);
        if (date.HasValue)
            filtered = filtered.Where(x => x.Value.StartTime.Date == date.Value.Date);
        
        return filtered.Select(x => x.Value).ToList();
    }

    public Tuple<int, Reservation> AddReservation(Reservation reservation)
    {
        int newId = Reservations.Count + 1;
        Reservations[newId] = reservation;

        return new Tuple<int, Reservation>(newId, reservation);
    }

    public Reservation? UpdateReservation(int id, Reservation reservation)
    {
        if (!Reservations.TryGetValue(id, out var existingReservation))
            return null;

        existingReservation.TableId = reservation.TableId;
        existingReservation.ClientId = reservation.ClientId;
        existingReservation.StartTime = reservation.StartTime;
        existingReservation.EndTime = reservation.EndTime;

        return existingReservation;
    }

    public bool DeleteReservation(int id)
    {
        return Reservations.Remove(id);
    }
}