using Events.Application; using Events.Domain; using Microsoft.EntityFrameworkCore;
namespace Events.Infrastructure;
public sealed class EventRepository(EventsDbContext db) : IEventRepository
{
    public Task<List<Event>> GetAllAsync() => db.Events.ToListAsync();
    public Task<Event?> GetAsync(Guid id) => db.Events.FindAsync(id).AsTask();
    public Task<List<Event>> GetTopAsync() => db.Events
        .OrderByDescending(item => item.TotalSeats == 0
            ? 0
            : (double)(item.TotalSeats - item.AvailableSeats) / item.TotalSeats)
        .Take(10)
        .ToListAsync();
    public Task AddAsync(Event item) => db.Events.AddAsync(item).AsTask();
    public void Delete(Event item) => db.Events.Remove(item);
    public Task SaveAsync() => db.SaveChangesAsync();
}
