using DNET.Backend.Api.Models;
using DNET.Backend.Api.Options;
using DNET.Backend.Api.Requests;
using DNET.Backend.DataAccess;
using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DNET.Backend.Api.Services;

public class ReservationService : IReservationService
{
    private readonly IOptionsMonitor<ReservationOptions> _optionsDelegate;
    private readonly TableReservationsDbContext _dbContext;

    public ReservationService(IOptionsMonitor<ReservationOptions> optionsDelegate, TableReservationsDbContext dbContext)
    {
        _optionsDelegate = optionsDelegate;
        _dbContext = dbContext;
    }

    public ReservationDTO? GetReservation(int id)
    {
        var reservation = _dbContext.Reservations
            .Include(r => r.ReservationDetail)
            .Include(r => r.Client)
            .Include(r => r.Table)
            .ThenInclude(t => t.Host)
            .FirstOrDefault(r => r.Id == id);
        
        if (reservation == null)
            return null;
    
        return new ReservationDTO(reservation);
    }
    
    public List<ReservationDTO> GetAllReservations(int? clientId = null, int? tableId = null, DateTime? date = null, String? reservationType = null)
    {
        var query = _dbContext.Reservations
            .Include(r => r.ReservationDetail)
            .Include(r => r.Client)
            .Include(r => r.Table)
            .ThenInclude(t => t.Host)
            .AsQueryable();
        
        if (clientId.HasValue)
            query = query.Where(r => r.ClientId == clientId);
        if (tableId.HasValue)
            query = query.Where(r => r.TableId == tableId);
        if (date.HasValue)
            query = query.Where(r => r.StartTime.Date == DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc));
        if (reservationType != null)
            query = query.Where(r => r.ReservationDetail.ReservationType.ToString() == reservationType);

        return query.Select(r => new ReservationDTO(r)).ToList();
    }

    public Tuple<int, ReservationDTO>? AddReservation(CreateUpdateReservationRequest request)
    {
        if (GetAllReservations(request.ClientId).Count >= _optionsDelegate.CurrentValue.ReservationLimit)
            return null;
        
        var table = _dbContext.Tables.FirstOrDefault(t => t.Number == request.TableNumber);
        var client = _dbContext.Clients.FirstOrDefault(c => c.Id == request.ClientId);
        if (table == null || client == null)
            throw new BadRequestException("The table or client doesn't exist", 400);
        
        var newStartTime = DateTime.SpecifyKind(DateTime.Parse(request.StartTime), DateTimeKind.Utc);
        var newEndTime = DateTime.SpecifyKind(DateTime.Parse(request.EndTime), DateTimeKind.Utc);
        var conflictingReservation = _dbContext.Reservations
            .FirstOrDefault(r => r.TableId == table.Id &&
                            r.StartTime < newEndTime && r.EndTime > newStartTime);
        if (conflictingReservation != null)
            throw new BadRequestException("The table is reserved for the time specified", 400);

        var reservationEntity = new ReservationEntity
        {
            Uid = Guid.NewGuid(),
            ClientId = request.ClientId,
            TableId = table.Id,
            StartTime = newStartTime,
            EndTime = newEndTime
        };
        _dbContext.Reservations.Add(reservationEntity);
        _dbContext.SaveChanges();

        var reservationDetailEntity = new ReservationDetailEntity
        {
            ReservationId = reservationEntity.Id,
            ReservationType = Enum.TryParse(request.ReservationType,
                out ReservationDetailEntity.ReservationTypeEnum reservationType)
                ? reservationType
                : ReservationDetailEntity.ReservationTypeEnum.Meeting,
            SpecialRequests = request.SpecialRequests
        };
        reservationEntity.ReservationDetail = reservationDetailEntity;
        _dbContext.ReservationDetails.Add(reservationDetailEntity);
        _dbContext.SaveChanges();
        
        return new Tuple<int, ReservationDTO>(reservationEntity.Id, new ReservationDTO(reservationEntity));
    }

    public ReservationDTO? UpdateReservation(int id, CreateUpdateReservationRequest request)
    {
        var existingReservation = _dbContext.Reservations.FirstOrDefault(r => r.Id == id);
        if (existingReservation == null)
            return null;
        
        var table = _dbContext.Tables.FirstOrDefault(t => t.Number == request.TableNumber);
        var client = _dbContext.Clients.FirstOrDefault(c => c.Id == request.ClientId);
        if (table == null || client == null)
            throw new BadRequestException("The table or client doesn't exist", 400);
        
        
        var newStartTime = DateTime.SpecifyKind(DateTime.Parse(request.StartTime), DateTimeKind.Utc);
        var newEndTime = DateTime.SpecifyKind(DateTime.Parse(request.EndTime), DateTimeKind.Utc);
        existingReservation.EndTime = DateTime.SpecifyKind(DateTime.Parse(request.EndTime), DateTimeKind.Utc);
        var conflictingReservation = _dbContext.Reservations
            .FirstOrDefault(r => r.Id != existingReservation.Id && r.TableId == table.Id &&
                            r.StartTime < newEndTime && r.EndTime > newStartTime);
        if (conflictingReservation != null)
            throw new BadRequestException("The table is reserved for the time specified", 400);

        existingReservation.TableId = table.Id;
        existingReservation.ClientId = request.ClientId;
        existingReservation.StartTime = newStartTime;
        existingReservation.EndTime = newEndTime;
        existingReservation.ReservationDetail.ReservationType = Enum.TryParse(request.ReservationType,
            out ReservationDetailEntity.ReservationTypeEnum reservationType)
            ? reservationType
            : ReservationDetailEntity.ReservationTypeEnum.Meeting;
        existingReservation.ReservationDetail.SpecialRequests = request.SpecialRequests;

        _dbContext.SaveChanges();

        return new ReservationDTO(existingReservation);
    }

    public bool DeleteReservation(int id)
    {
        var reservation = _dbContext.Reservations.FirstOrDefault(r => r.Id == id);

        if (reservation == null)
            return false;

        _dbContext.Reservations.Remove(reservation);
        _dbContext.SaveChanges();

        return true;
    }
}