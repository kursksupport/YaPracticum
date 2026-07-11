using EventManagementService.DTOs;
using EventManagementService.Models;

namespace EventManagementService.Services;

public interface IEventService
{
    Task<PaginatedResult> GetAllAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize);

    Task<Event?> GetByIdAsync(Guid id);

    Task<EventInfoDto> CreateAsync(CreateEventDto createEventDto);

    Task<bool> UpdateAsync(Guid id, Event updatedEvent);

    Task<bool> DeleteAsync(Guid id);
}