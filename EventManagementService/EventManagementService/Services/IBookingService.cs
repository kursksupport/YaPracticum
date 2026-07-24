using EventManagementService.Domain.Entities;
using EventManagementService.Domain.Enums;

namespace EventManagementService.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid eventId);

    Task<Booking?> GetBookingByIdAsync(Guid bookingId);

    Task<List<Booking>> GetPendingBookingsAsync();

    Task UpdateBookingAsync(Booking booking);
}