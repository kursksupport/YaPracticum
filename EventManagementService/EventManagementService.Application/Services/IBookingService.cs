using EventManagementService.Domain.Entities;

namespace EventManagementService.Application.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Guid eventId, Guid userId);

    Task<Booking?> GetBookingByIdAsync(Guid bookingId);

    Task CancelBookingAsync(
        Guid bookingId,
        Guid userId,
        EventManagementService.Domain.Enums.UserRole userRole);

    Task<List<Booking>> GetPendingBookingsAsync();
}
