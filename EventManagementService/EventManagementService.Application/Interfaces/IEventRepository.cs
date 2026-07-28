using EventManagementService.Domain.Entities;

namespace EventManagementService.Application.Interfaces;

public interface IEventRepository
{
    Task<(List<Event> Items, int TotalCount)> GetAllAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize);

    Task<Event?> GetByIdAsync(Guid id);

    Task AddAsync(Event eventItem);

    Task DeleteAsync(Event eventItem);

    Task SaveChangesAsync();
}