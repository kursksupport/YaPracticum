using EventManagementService.Application.Interfaces;
using EventManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Infrastructure.DataAccess.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Event> Items, int TotalCount)> GetAllAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize)
    {
        var query = _context.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(e =>
                e.Title.Contains(title));
        }

        if (from.HasValue)
        {
            query = query.Where(e =>
                e.StartAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e =>
                e.EndAt <= to.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }


    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id);
    }


    public async Task AddAsync(Event eventItem)
    {
        await _context.Events.AddAsync(eventItem);
    }


    public async Task DeleteAsync(Event eventItem)
    {
        _context.Events.Remove(eventItem);
        await Task.CompletedTask;
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}