using EventManagementService.DataAccess.Repositories;
using EventManagementService.DTOs;
using EventManagementService.Models;


namespace EventManagementService.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;


    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }
    public async Task<PaginatedResult> GetAllAsync(
    string? title,
    DateTime? from,
    DateTime? to,
    int page,
    int pageSize)
    {
        var result = await _repository.GetAllAsync(
            title,
            from,
            to,
            page,
            pageSize);

        return new PaginatedResult
        {
            TotalCount = result.TotalCount,
            Items = result.Items,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
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

        await _repository.AddAsync(eventItem);

        await _repository.SaveChangesAsync();

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
        var existingEvent = await _repository.GetByIdAsync(id);

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

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var eventItem = await GetByIdAsync(id);

        if (eventItem == null)
        {
            return false;
        }

        await _repository.DeleteAsync(eventItem);

        await _repository.SaveChangesAsync();

        return true;
    }
}