using Microsoft.AspNetCore.Mvc;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Requests;
using DNET.Backend.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace DNET.Backend.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(IReservationService reservationService, ILogger<ReservationController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }
    
    // GET /reservations/1
    [HttpGet]
    [Route("{id}")]
    [Authorize(Roles = "User, Admin")]
    public IActionResult GetReservation(int id)
    {
        _logger.LogInformation("Fetching reservation with ID={Id}", id);
        
        var reservation = _reservationService.GetReservation(id);
        if (reservation == null)
            return NotFound(new ErrorResponse { Message = "Reservation not found" });
    
        _logger.LogInformation("Fetched reservation with ID {Id} for table No{TableNumber} {StartTime} – {EndTime}", 
            id, reservation.TableNumber, reservation.StartTime, reservation.EndTime);
        
        return Ok(reservation);
    }
    
    // GET /reservations?tableId=1&date=2025-02-29&reservationType=Birthday
    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    public IActionResult GetReservations(int? clientId, int? tableNumber, DateTime? date, String? reservationType)
    {
        _logger.LogInformation(
            "Fetching reservations for clientId={ClientId}, tableNumber={TableNumber}, date={Date}, reservationType={ReservationType}",
            clientId, tableNumber, date?.ToString("yyyy-MM-dd"), reservationType);
        
        var reservations = _reservationService.GetAllReservations(clientId, tableNumber, date, reservationType);
        if (!reservations.Any())
            return NotFound(new ErrorResponse { Message = "Reservations not found" });
        
        _logger.LogInformation(
            "Fetched {Count} reservations for clientId={ClientId}, tableNumber={TableNumber}, date={Date}, reservationType={ReservationType}",
            reservations.Count, clientId, tableNumber, date?.ToString("yyyy-MM-dd"), reservationType);
        
        return Ok(reservations);
    }
    
    // POST /reservations
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateReservation(CreateUpdateReservationRequest request)
    {
        _logger.LogInformation("Creating reservation for table No{TableNumber} {StartTime} – {EndTime}", 
            request.TableNumber, request.StartTime, request.EndTime);
        
        var result = _reservationService.AddReservation(request);
        if (result == null)
            return BadRequest(new ErrorResponse { Message = "Reservation limit exceeded" });
        
        _logger.LogInformation("Created reservation for table No{TableNumber} {StartTime} – {EndTime} with ID={Id}", 
            result.Item2.TableNumber, result.Item2.StartTime, result.Item2.EndTime, result.Item1);

        return Created($"/reservations/{result.Item1}", result.Item2);
    }

    // PUT /reservations/1
    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateReservation(int id, CreateUpdateReservationRequest request)
    {
        _logger.LogInformation("Updating reservation for table No{TableNumber} {StartTime} – {EndTime} with ID={Id}", 
            request.TableNumber, request.StartTime, request.EndTime, id);
        
        var updatedReservation = _reservationService.UpdateReservation(id, request);
        if (updatedReservation == null)
            return NotFound(new ErrorResponse { Message = "Reservation, table or client not found" });

        _logger.LogInformation("Updated reservation for table No{TableNumber} {StartTime} – {EndTime} with ID={Id}", 
            updatedReservation.TableNumber, updatedReservation.StartTime, updatedReservation.EndTime, id);
        
        return Ok(updatedReservation);
     }

    // DELETE /reservations/1
    [HttpDelete]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteReservation(int id)
    {
        _logger.LogInformation("Deleting reservation with ID={Id}", id);

        if (!_reservationService.DeleteReservation(id))
            return NotFound(new ErrorResponse { Message = "Reservation not found" });
        
        _logger.LogInformation("Deleted reservation with ID={Id}", id);
        
        return NoContent();
    }
}