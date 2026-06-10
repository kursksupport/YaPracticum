using EventManagementService.DTOs;
using EventManagementService.Models;

namespace EventManagementService.Services
{
    public interface IEventService
    {
        PaginatedResult GetAll(string? title, DateTime? from, DateTime? to, int page, int pageSize);

        public Event? GetById(Guid id);

        Event Create(Event eventItem);

        public bool Update(Guid id, Event updatedEvent);

        public bool Delete(Guid id);
    }
}
