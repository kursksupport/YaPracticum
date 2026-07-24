using EventManagementService.Domain.Entities;
using EventManagementService.Domain.Enums;

namespace EventManagementService.DataAccess.Repositories;

public interface IBookingRepository
{
    Task AddAsync(Booking booking);

    Task<Booking?> GetByIdAsync(Guid id);

    Task<List<Booking>> GetPendingAsync();

    Task SaveChangesAsync();
}