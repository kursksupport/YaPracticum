using EventManagementService.DTOs;
using EventManagementService.Models;
using System.Xml.Linq;

namespace EventManagementService.Services
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new();


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

        public Event? GetById(Guid id)
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }

        public EventInfoDto Create(CreateEventDto createEventDto)
        {
            var eventItem = Event.Create(
                createEventDto.Title,
                createEventDto.Description,
                createEventDto.StartAt,
                createEventDto.EndAt,
                createEventDto.TotalSeats!.Value);

            _events.Add(eventItem);

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

        public bool Update(Guid id, Event updatedEvent)
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

            if (updatedEvent.TotalSeats > 0)
            {
                existingEvent.UpdateSeats(updatedEvent.TotalSeats);
            }

            return true;
        }

        public bool Delete(Guid id)
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
