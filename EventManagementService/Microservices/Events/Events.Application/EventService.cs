using Events.Domain;
namespace Events.Application;
public record EventRequest(string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats);
public enum SeatsDecreaseResult { Success, EventNotFound, NotEnoughSeats }
public interface IEventRepository { Task<List<Event>> GetAllAsync(); Task<Event?> GetAsync(Guid id); Task AddAsync(Event item); void Delete(Event item); Task SaveAsync(); }
public interface IEventService { Task<List<Event>> GetAllAsync(); Task<Event?> GetAsync(Guid id); Task<Event> CreateAsync(EventRequest request); Task<bool> UpdateAsync(Guid id, EventRequest request); Task<bool> DeleteAsync(Guid id); Task<SeatsDecreaseResult> DecreaseAvailableSeatsAsync(Guid eventId, int seatsCount); }
public sealed class EventService(IEventRepository repository) : IEventService
{
    public Task<List<Event>> GetAllAsync() => repository.GetAllAsync(); public Task<Event?> GetAsync(Guid id) => repository.GetAsync(id);
    public async Task<Event> CreateAsync(EventRequest r) { var item = Event.Create(r.Title, r.Description, r.StartAt, r.EndAt, r.TotalSeats); await repository.AddAsync(item); await repository.SaveAsync(); return item; }
    public async Task<bool> UpdateAsync(Guid id, EventRequest r) { var item = await repository.GetAsync(id); if (item is null) return false; item.Update(r.Title, r.Description, r.StartAt, r.EndAt, r.TotalSeats); await repository.SaveAsync(); return true; }
    public async Task<bool> DeleteAsync(Guid id) { var item = await repository.GetAsync(id); if (item is null) return false; repository.Delete(item); await repository.SaveAsync(); return true; }
    public async Task<SeatsDecreaseResult> DecreaseAvailableSeatsAsync(Guid eventId, int seatsCount)
    {
        var item = await repository.GetAsync(eventId);
        if (item is null) return SeatsDecreaseResult.EventNotFound;
        if (!item.TryTakeSeats(seatsCount)) return SeatsDecreaseResult.NotEnoughSeats;

        await repository.SaveAsync();
        return SeatsDecreaseResult.Success;
    }
}
