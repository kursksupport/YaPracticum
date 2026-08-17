using Bookings.Domain;
namespace Bookings.Application;
public interface IBookingRepository { Task<Booking?> GetAsync(Guid id); Task AddAsync(Booking booking); Task SaveAsync(); }
public interface IBookingService { Task<Booking> CreateAsync(Guid eventId, Guid userId); Task<Booking?> GetAsync(Guid id); Task<bool> CancelAsync(Guid id, Guid userId, bool isAdmin); }
public sealed class BookingService(IBookingRepository repository) : IBookingService
{
    // Events and Users databases are intentionally not accessed here.
    public async Task<Booking> CreateAsync(Guid eventId, Guid userId) { var booking = Booking.Create(eventId, userId); await repository.AddAsync(booking); await repository.SaveAsync(); return booking; }
    public Task<Booking?> GetAsync(Guid id) => repository.GetAsync(id);
    public async Task<bool> CancelAsync(Guid id, Guid userId, bool isAdmin) { var booking = await repository.GetAsync(id); if (booking is null) return false; if (!isAdmin && booking.UserId != userId) throw new UnauthorizedAccessException(); booking.Cancel(); await repository.SaveAsync(); return true; }
}
