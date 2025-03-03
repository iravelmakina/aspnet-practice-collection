using Microsoft.AspNetCore.Mvc;

namespace DNET.Backend.Api.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationController : ControllerBase
{
    private readonly Dictionary<int, Reservation> _reservations;

    public ReservationController(Dictionary<int, Reservation> reservations)
    {
        _reservations = reservations;
    }
    
    // GET /reservations/1
    [HttpGet]
    [Route("{id}")]
    public IActionResult GetReservation(int id)
    {
        if (!_reservations.TryGetValue(id, out var reservation))
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
    
        var filtered = _reservations.AsEnumerable();

        if (tableId.HasValue)
            filtered = _reservations.Where(x => x.Value.TableId == tableId);
        if (date.HasValue)
            filtered = filtered.Where(x => x.Value.StartTime.Date == date.Value.Date);
        
        var resultList = filtered.Select(x => x.Value).ToList();
        if (resultList.Count == 0)
            return NotFound(new ErrorResponse { Message = "Reservations not found" });

        return Ok(resultList);
    }
    
    // POST /reservations
    [HttpPost]
    public IActionResult PostReservation(Reservation reservation)
    {
        _reservations.Add(_reservations.Count + 1, reservation);
        return Created($"/reservations/{_reservations.Count}", reservation);
    }

    // PUT /reservations/1
    [HttpPut]
    [Route("{id}")]
    public IActionResult PutReservation(int id, Reservation reservation)
    {
        if (!_reservations.TryGetValue(id, out var existingReservation))
            return NotFound(new ErrorResponse { Message = "Reservation not found" });

        existingReservation.TableId = reservation.TableId;
        existingReservation.ClientId = reservation.ClientId;
        existingReservation.StartTime = reservation.StartTime;
        existingReservation.EndTime = reservation.EndTime;
        
        return Ok(existingReservation);
    }

    // DELETE /reservations/1
    [HttpDelete]
    [Route("{id}")]
    public IActionResult DeleteReservation(int id)
    {
        if (!_reservations.Remove(id))
            return NotFound(new ErrorResponse { Message = "Reservation not found" });
        
        return NoContent();
    }
}