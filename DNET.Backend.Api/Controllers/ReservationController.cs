using Microsoft.AspNetCore.Mvc;
using DNET.Backend.Api.Models;
using DNET.Backend.Api.Services;

namespace DNET.Backend.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
    
    // GET /reservations/1
    [HttpGet]
    [Route("{id}")]
    public IActionResult GetReservation(int id)
    {
        var reservation = _reservationService.GetReservation(id);
        if (reservation == null)
            return NotFound(new ErrorResponse { Message = "Reservation not found" });
    
        return Ok(reservation);
    }
    
    // GET /reservations?tableId=1&date=2025-02-29
    [HttpGet]
    public IActionResult GetReservations(int? tableId, DateTime? date)
    {
        // var validParameters = new HashSet<string> { "tableId", "date" };
        // var queryParameters = context.Request.Query.Keys;
        // foreach (var param in queryParameters)
        // {
        //     if (!validParameters.Contains(param))
        //     {
        //         return BadRequest(new ErrorResponse { Message = $"Invalid query parameter: {param}" });
        //     }
        // }

        var reservations = _reservationService.GetAllReservations(tableId, date);
        if (reservations == null)
            return NotFound(new ErrorResponse { Message = "Reservations not found" });

        return Ok(reservations);
    }
    
    // POST /reservations
    [HttpPost]
    public IActionResult CreateReservation(Reservation reservation)
    {
        var (id, newReservation) = _reservationService.AddReservation(reservation);
        return Created($"/reservations/{id}", reservation);
    }

    // PUT /reservations/1
    [HttpPut]
    [Route("{id}")]
    public IActionResult UpdateReservation(int id, Reservation reservation)
    {
        var updatedReservation = _reservationService.UpdateReservation(id, reservation);
        if (updatedReservation == null)
            return NotFound(new ErrorResponse { Message = "Reservation not found" });
        
        return Ok(updatedReservation);
    }

    // DELETE /reservations/1
    [HttpDelete]
    [Route("{id}")]
    public IActionResult DeleteReservation(int id)
    {
        if (!_reservationService.DeleteReservation(id))
            return NotFound(new ErrorResponse { Message = "Reservation not found" });
        
        return NoContent();
    }
}