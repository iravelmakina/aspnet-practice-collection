using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using Microsoft.Extensions.Options;

namespace DNET.Backend.Api.Services;

public class ReservationService : IReservationService
{
    private Dictionary<int, Reservation> Reservations  { get; set; }
    private readonly IOptionsMonitor<ReservationOptions> _optionsDelegate;

    public ReservationService(IOptionsMonitor<ReservationOptions> optionsDelegate)
    {
        Reservations = new Dictionary<int, Reservation>();
        _optionsDelegate = optionsDelegate;
    }

    public Reservation? GetReservation(int id)
    {
        if (!Reservations.TryGetValue(id, out var reservation))
            return null;
    
        return reservation;
    }
    
    public List<Reservation> GetAllReservations(int? clientId = null, int? tableId = null, DateTime? date = null)
    {
        var filtered = Reservations.AsEnumerable();

        if (clientId.HasValue)
            filtered = Reservations.Where(x => x.Value.ClientId == clientId);
        if (tableId.HasValue)
            filtered = Reservations.Where(x => x.Value.TableId == tableId);
        if (date.HasValue)
            filtered = filtered.Where(x => x.Value.StartTime.Date == date.Value.Date);
        
        return filtered.Select(x => x.Value).ToList();
    }

    public Tuple<int, Reservation>? AddReservation(Reservation reservation)
    {
        if (GetAllReservations(reservation.ClientId).Count >= _optionsDelegate.CurrentValue.ReservationLimit)
            return null;
        
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