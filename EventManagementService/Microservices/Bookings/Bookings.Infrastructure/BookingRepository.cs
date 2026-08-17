using Bookings.Application; using Bookings.Domain;
namespace Bookings.Infrastructure;
public sealed class BookingRepository(BookingsDbContext db) : IBookingRepository { public Task<Booking?> GetAsync(Guid id) => db.Bookings.FindAsync(id).AsTask(); public Task AddAsync(Booking booking) => db.Bookings.AddAsync(booking).AsTask(); public Task SaveAsync() => db.SaveChangesAsync(); }
