using EventManagementService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers;

[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("events/{id:guid}/book")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBooking(Guid id)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(id);

            return Accepted(
                $"/bookings/{booking.Id}",
                booking);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("bookings/{id:guid}")]
    public async Task<IActionResult> GetBookingById(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);

        if (booking == null)
        {
            return NotFound();
        }

        return Ok(booking);
    }
}

