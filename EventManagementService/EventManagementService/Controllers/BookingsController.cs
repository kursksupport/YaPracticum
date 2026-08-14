using System.Security.Claims;
using EventManagementService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers;

[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(
        IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("events/{id:guid}/book")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBooking(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var booking =
            await _bookingService.CreateBookingAsync(id, userId);

        return Accepted(
            $"/bookings/{booking.Id}",
            booking);
    }

    [HttpGet("bookings/{id:guid}")]
    public async Task<IActionResult> GetBookingById(Guid id)
    {
        var booking =
            await _bookingService.GetBookingByIdAsync(id);

        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }
}