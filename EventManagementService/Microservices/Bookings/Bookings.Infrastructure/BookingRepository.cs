using Bookings.Application; using Bookings.Domain; using Microsoft.EntityFrameworkCore;
namespace Bookings.Infrastructure;
public sealed class BookingRepository(BookingsDbContext db) : IBookingRepository { public Task<Booking?> GetAsync(Guid id) => db.Bookings.FindAsync(id).AsTask(); public Task<List<Booking>> GetPendingAsync() => db.Bookings.Where(x => x.Status == BookingStatus.Pending).ToListAsync(); public Task AddAsync(Booking booking) => db.Bookings.AddAsync(booking).AsTask(); public Task SaveAsync() => db.SaveChangesAsync(); }
