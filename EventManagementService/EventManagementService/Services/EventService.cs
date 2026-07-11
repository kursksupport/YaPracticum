using EventManagementService.DataAccess;
using EventManagementService.DTOs;
using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _context;


    public EventService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult> GetAllAsync(
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
            query = query.Where(e => e.StartAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.EndAt <= to.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult
        {
            TotalCount = totalCount,
            Items = items,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<EventInfoDto> CreateAsync(
        CreateEventDto createEventDto)
    {
        var eventItem = Event.Create(
            createEventDto.Title,
            createEventDto.Description,
            createEventDto.StartAt,
            createEventDto.EndAt,
            createEventDto.TotalSeats!.Value);

        _context.Events.Add(eventItem);

        await _context.SaveChangesAsync();

        return new EventInfoDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            StartAt = eventItem.StartAt,
            EndAt = eventItem.EndAt,
            TotalSeats = eventItem.TotalSeats,
            AvailableSeats = eventItem.AvailableSeats
        };
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        Event updatedEvent)
    {
        var existingEvent = await GetByIdAsync(id);

        if (existingEvent == null)
        {
            return false;
        }

        existingEvent.Title = updatedEvent.Title;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.StartAt = updatedEvent.StartAt;
        existingEvent.EndAt = updatedEvent.EndAt;

        if (updatedEvent.TotalSeats > 0)
        {
            existingEvent.UpdateSeats(updatedEvent.TotalSeats);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var eventItem = await GetByIdAsync(id);

        if (eventItem == null)
        {
            return false;
        }

        _context.Events.Remove(eventItem);

        await _context.SaveChangesAsync();

        return true;
    }
}