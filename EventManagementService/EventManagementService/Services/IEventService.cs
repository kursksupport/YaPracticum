using EventManagementService.DTOs;
using EventManagementService.Models;

namespace EventManagementService.Services
{
    public interface IEventService
    {
        PaginatedResult GetAll(string? title, DateTime? from, DateTime? to, int page, int pageSize);

        Event? GetById(int id);

        Event Create(Event eventItem);

        bool Update(int id, Event updatedEvent);

        bool Delete(int id);
    }
}
