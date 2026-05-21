using EventManagementService.Models;
using System.Xml.Linq;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new();

        private int _nextId = 1;

        public List<Event> GetAll()
        {
            return _events;
        }

        public Event? GetById(int id)
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }

        public Event Create(Event eventItem)
        {
            eventItem.Id = _nextId;

            _nextId++;

            _events.Add(eventItem);

            return eventItem;
        }

        public bool Update(int id, Event updatedEvent)
        {
            var existingEvent = GetById(id);

            if (existingEvent == null)
            {
                return false;
            }

            existingEvent.Title = updatedEvent.Title;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.StartAt = updatedEvent.StartAt;
            existingEvent.EndAt = updatedEvent.EndAt;

            return true;
        }

        public bool Delete(int id)
        {
            var eventItem = GetById(id);

            if (eventItem == null)
            {
                return false;
            }

            _events.Remove(eventItem);

            return true;
        }
    }
}
