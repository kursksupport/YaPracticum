using EventManagementService.Models;

namespace EventManagementService.Services
{
    public interface IEventService
    {
        List<Event> GetAll();

        Event? GetById(int id);

        Event Create(Event eventItem);

        bool Update(int id, Event updatedEvent);

        bool Delete(int id);
    }
}
