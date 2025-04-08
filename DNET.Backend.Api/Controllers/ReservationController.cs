using DNET.Backend.Api.Infrastructure;
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

    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
    
    // GET /reservations/1
    [HttpGet]
    [Route("{id}")]
    [Authorize(Roles = "User, Admin")]
    public IActionResult GetReservation(int id)
    {
        var reservation = _reservationService.GetReservation(id);
        if (reservation == null)
            return NotFound(new ErrorResponse { Message = "Reservation not found" });
    
        return Ok(reservation);
    }
    
    // GET /reservations?tableId=1&date=2025-02-29&reservationType=Birthday
    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    public IActionResult GetReservations(int? clientId, int? tableNumber, DateTime? date, String? reservationType)
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

        var reservations = _reservationService.GetAllReservations(clientId, tableNumber, date, reservationType);
        if (!reservations.Any())
            return NotFound(new ErrorResponse { Message = "Reservations not found" });

        return Ok(reservations);
    }
    
    // POST /reservations
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateReservation(CreateUpdateReservationRequest request)
    {
        try
        {
            var result = _reservationService.AddReservation(request);
            if (result == null)
                return BadRequest(new ErrorResponse { Message = "Reservation limit exceeded" });

            return Created($"/reservations/{result.Item1}", result.Item2);
        } catch (ServerException badRequestException)
        {
            return BadRequest(new ErrorResponse
            {
                Message = badRequestException.WrongMessage,
                Status = badRequestException.WrongCode
            });
        }
    }

    // PUT /reservations/1
    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateReservation(int id, CreateUpdateReservationRequest request)
    {
        try {
            var updatedReservation = _reservationService.UpdateReservation(id, request);
            if (updatedReservation == null)
                return NotFound(new ErrorResponse { Message = "Reservation, table or client not found" });

            return Ok(updatedReservation);
        } catch (ServerException badRequestException)
        {
            return BadRequest(new ErrorResponse
            {
                Message = badRequestException.WrongMessage,
                Status = badRequestException.WrongCode
            });
        }
    }

    // DELETE /reservations/1
    [HttpDelete]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteReservation(int id)
    {
        if (!_reservationService.DeleteReservation(id))
            return NotFound(new ErrorResponse { Message = "Reservation not found" });
        
        return NoContent();
    }
}