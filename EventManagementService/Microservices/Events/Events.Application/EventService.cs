using Events.Domain;
namespace Events.Application;
public record EventRequest(string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats);
public record EventResponse(Guid Id, string Title, string? Description, DateTime StartAt, DateTime EndAt, int TotalSeats, int AvailableSeats);
public enum SeatsDecreaseResult { Success, EventNotFound, NotEnoughSeats }
public interface IEventRepository { Task<List<Event>> GetAllAsync(); Task<Event?> GetAsync(Guid id); Task<List<Event>> GetTopAsync(); Task AddAsync(Event item); void Delete(Event item); Task SaveAsync(); }
public interface IEventService { Task<List<Event>> GetAllAsync(); Task<EventResponse?> GetAsync(Guid id); Task<List<EventResponse>> GetTopAsync(); Task<Event> CreateAsync(EventRequest request); Task<bool> UpdateAsync(Guid id, EventRequest request); Task<bool> DeleteAsync(Guid id); Task<SeatsDecreaseResult> DecreaseAvailableSeatsAsync(Guid eventId, int seatsCount); }
public sealed class EventService(
    IEventRepository repository,
    ICacheService cache,
    CacheSettings cacheSettings) : IEventService
{
    public Task<List<Event>> GetAllAsync() => repository.GetAllAsync();

    public async Task<EventResponse?> GetAsync(Guid id)
    {
        var cacheKey = GetEventCacheKey(id);
        var cachedEvent = await cache.GetAsync<EventResponse>(cacheKey);
        if (cachedEvent is not null)
            return cachedEvent;

        var item = await repository.GetAsync(id);
        if (item is null)
            return null;

        var response = ToResponse(item);
        await cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(cacheSettings.EventTtlMinutes));
        return response;
    }

    public async Task<List<EventResponse>> GetTopAsync()
    {
        const string cacheKey = "events:top10";
        var cachedEvents = await cache.GetAsync<List<EventResponse>>(cacheKey);
        if (cachedEvents is not null)
            return cachedEvents;

        var events = await repository.GetTopAsync();
        var response = events.Select(ToResponse).ToList();
        await cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(cacheSettings.TopEventsTtlMinutes));
        return response;
    }

    public async Task<Event> CreateAsync(EventRequest r)
    {
        var item = Event.Create(r.Title, r.Description, r.StartAt, r.EndAt, r.TotalSeats);
        await repository.AddAsync(item);
        await repository.SaveAsync();
        return item;
    }

    public async Task<bool> UpdateAsync(Guid id, EventRequest r)
    {
        var item = await repository.GetAsync(id);
        if (item is null)
            return false;

        item.Update(r.Title, r.Description, r.StartAt, r.EndAt, r.TotalSeats);
        await repository.SaveAsync();
        await cache.RemoveAsync(GetEventCacheKey(id));
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await repository.GetAsync(id);
        if (item is null)
            return false;

        repository.Delete(item);
        await repository.SaveAsync();
        await cache.RemoveAsync(GetEventCacheKey(id));
        return true;
    }

    public async Task<SeatsDecreaseResult> DecreaseAvailableSeatsAsync(Guid eventId, int seatsCount)
    {
        var item = await repository.GetAsync(eventId);
        if (item is null) return SeatsDecreaseResult.EventNotFound;
        if (!item.TryTakeSeats(seatsCount)) return SeatsDecreaseResult.NotEnoughSeats;

        await repository.SaveAsync();
        await cache.RemoveAsync(GetEventCacheKey(eventId));
        return SeatsDecreaseResult.Success;
    }

    private static string GetEventCacheKey(Guid id) => $"event:{id}";

    private static EventResponse ToResponse(Event item) => new(
        item.Id,
        item.Title,
        item.Description,
        item.StartAt,
        item.EndAt,
        item.TotalSeats,
        item.AvailableSeats);
}
