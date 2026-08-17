using System.Security.Claims; 
using Bookings.Application; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;

namespace Bookings.Api;
[ApiController, Authorize] public sealed class BookingsController(IBookingService service) : ControllerBase
{
    [HttpPost("events/{eventId:guid}/book")] 
    public async Task<IActionResult> Create(Guid eventId) 
    { 
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) 
            return Unauthorized(); 
        var booking = await service.CreateAsync(eventId, userId); 
        return Accepted($"/bookings/{booking.Id}", booking); 
    }
    [HttpGet("bookings/{id:guid}")] 
    public async Task<IActionResult> Get(Guid id) => await service.GetAsync(id) is { } item ? Ok(item) : NotFound();
    [HttpDelete("bookings/{id:guid}")] 
    public async Task<IActionResult> Cancel(Guid id) 
    { 
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) 
            return Unauthorized(); 
        return await service.CancelAsync(id, userId, User.IsInRole("Admin")) ? NoContent() : NotFound(); 
    }
}
