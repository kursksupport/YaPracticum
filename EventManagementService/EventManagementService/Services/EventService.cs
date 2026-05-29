using EventManagementService.DTOs;
using EventManagementService.Models;
using System.Xml.Linq;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new();

        private int _nextId = 1;

        public PaginatedResult GetAll(string? title, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var query = _events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(e =>
                    e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.StartAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.EndAt <= to.Value);
            }

            var totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResult
            {
                TotalCount = totalCount,
                Items = items,
                Page = page,
                PageSize = pageSize
            };
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
