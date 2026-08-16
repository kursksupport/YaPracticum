using EventManagementService.Domain.Entities;

namespace EventManagementService.Application.Interfaces;

public interface IBookingRepository
{
    Task AddAsync(Booking booking);

    Task<Booking?> GetByIdAsync(Guid id);

    Task<List<Booking>> GetPendingAsync();

    Task<int> CountActiveByUserIdAsync(Guid userId);

    Task SaveChangesAsync();
}