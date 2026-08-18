using Bookings.Domain;
using Contracts;
namespace Bookings.Application;
public interface IBookingRepository { Task<Booking?> GetAsync(Guid id); Task<List<Booking>> GetPendingAsync(); Task AddAsync(Booking booking); Task SaveAsync(); }
public interface IBookingConfirmedPublisher { Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken); }
public interface IBookingService { Task<Booking> CreateAsync(Guid eventId, Guid userId); Task<Booking?> GetAsync(Guid id); Task<bool> CancelAsync(Guid id, Guid userId, bool isAdmin); Task ConfirmPendingAsync(CancellationToken cancellationToken); }
public sealed class BookingService(IBookingRepository repository, IBookingConfirmedPublisher publisher) : IBookingService
{
    // Events and Users databases are intentionally not accessed here.
    public async Task<Booking> CreateAsync(Guid eventId, Guid userId) { var booking = Booking.Create(eventId, userId); await repository.AddAsync(booking); await repository.SaveAsync(); return booking; }
    public Task<Booking?> GetAsync(Guid id) => repository.GetAsync(id);
    public async Task<bool> CancelAsync(Guid id, Guid userId, bool isAdmin) { var booking = await repository.GetAsync(id); if (booking is null) return false; if (!isAdmin && booking.UserId != userId) throw new UnauthorizedAccessException(); booking.Cancel(); await repository.SaveAsync(); return true; }
    public async Task ConfirmPendingAsync(CancellationToken cancellationToken)
    {
        var bookings = await repository.GetPendingAsync();
        foreach (var booking in bookings)
        {
            booking.Confirm();
            await repository.SaveAsync();
            await publisher.PublishAsync(new BookingConfirmed(booking.Id, booking.EventId, booking.UserId, 1, DateTime.UtcNow), cancellationToken);
        }
    }
}
