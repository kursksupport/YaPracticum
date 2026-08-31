using Events.Application;
using Events.Domain;

namespace Events.Application.Tests;

public class EventServiceCacheTests
{
    [Fact]
    public async Task GetAsync_WhenCacheHit_DoesNotCallRepository()
    {
        var id = Guid.NewGuid();
        var cachedEvent = new EventResponse(
            id, "Концерт", null, DateTime.UtcNow, DateTime.UtcNow.AddHours(2), 100, 40);
        var repository = new EventRepositoryStub();
        var cache = new CacheServiceStub();
        cache.Values[$"event:{id}"] = cachedEvent;
        var service = CreateService(repository, cache);

        var result = await service.GetAsync(id);

        Assert.Equal(cachedEvent, result);
        Assert.Equal(0, repository.GetCalls);
    }

    [Fact]
    public async Task GetAsync_WhenCacheMiss_GetsEventFromRepositoryAndSavesToCache()
    {
        var eventItem = CreateEvent();
        var repository = new EventRepositoryStub { EventToReturn = eventItem };
        var cache = new CacheServiceStub();
        var service = CreateService(repository, cache);

        var result = await service.GetAsync(eventItem.Id);

        Assert.NotNull(result);
        Assert.Equal(eventItem.Id, result.Id);
        Assert.Equal(1, repository.GetCalls);
        Assert.Equal($"event:{eventItem.Id}", cache.LastSetKey);
        Assert.Equal(TimeSpan.FromMinutes(10), cache.LastExpiration);
    }

    [Fact]
    public async Task GetTopAsync_WhenCacheHit_DoesNotCallRepository()
    {
        var cachedEvents = new List<EventResponse>
        {
            new(Guid.NewGuid(), "Фестиваль", null, DateTime.UtcNow, DateTime.UtcNow.AddHours(3), 100, 10)
        };
        var repository = new EventRepositoryStub();
        var cache = new CacheServiceStub();
        cache.Values["events:top10"] = cachedEvents;
        var service = CreateService(repository, cache);

        var result = await service.GetTopAsync();

        Assert.Same(cachedEvents, result);
        Assert.Equal(0, repository.GetTopCalls);
    }

    [Fact]
    public async Task GetTopAsync_WhenCacheMiss_GetsEventsFromRepositoryAndSavesToCache()
    {
        var repository = new EventRepositoryStub
        {
            TopEventsToReturn = new List<Event> { CreateEvent() }
        };
        var cache = new CacheServiceStub();
        var service = CreateService(repository, cache);

        var result = await service.GetTopAsync();

        Assert.Single(result);
        Assert.Equal(1, repository.GetTopCalls);
        Assert.Equal("events:top10", cache.LastSetKey);
        Assert.Equal(TimeSpan.FromMinutes(5), cache.LastExpiration);
    }

    [Fact]
    public async Task UpdateAsync_InvalidatesCacheAfterSavingChanges()
    {
        var operations = new List<string>();
        var eventItem = CreateEvent();
        var repository = new EventRepositoryStub(operations) { EventToReturn = eventItem };
        var cache = new CacheServiceStub(operations);
        var service = CreateService(repository, cache);
        var request = new EventRequest(
            "Новое название", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), 120);

        var result = await service.UpdateAsync(eventItem.Id, request);

        Assert.True(result);
        Assert.Equal(new[] { "get", "save", $"remove:event:{eventItem.Id}" }, operations);
    }

    [Fact]
    public async Task DeleteAsync_InvalidatesCacheAfterDeletingEvent()
    {
        var operations = new List<string>();
        var eventItem = CreateEvent();
        var repository = new EventRepositoryStub(operations) { EventToReturn = eventItem };
        var cache = new CacheServiceStub(operations);
        var service = CreateService(repository, cache);

        var result = await service.DeleteAsync(eventItem.Id);

        Assert.True(result);
        Assert.Equal(new[] { "get", "delete", "save", $"remove:event:{eventItem.Id}" }, operations);
    }

    [Fact]
    public async Task DecreaseAvailableSeatsAsync_InvalidatesCacheAfterSavingChanges()
    {
        var operations = new List<string>();
        var eventItem = CreateEvent();
        var repository = new EventRepositoryStub(operations) { EventToReturn = eventItem };
        var cache = new CacheServiceStub(operations);
        var service = CreateService(repository, cache);

        var result = await service.DecreaseAvailableSeatsAsync(eventItem.Id, 2);

        Assert.Equal(SeatsDecreaseResult.Success, result);
        Assert.Equal(new[] { "get", "save", $"remove:event:{eventItem.Id}" }, operations);
    }

    private static EventService CreateService(EventRepositoryStub repository, CacheServiceStub cache) =>
        new(repository, cache, new CacheSettings
        {
            EventTtlMinutes = 10,
            TopEventsTtlMinutes = 5
        });

    private static Event CreateEvent() => Event.Create(
        "Тестовое событие",
        "Описание",
        DateTime.UtcNow,
        DateTime.UtcNow.AddHours(2),
        100);
}

internal sealed class EventRepositoryStub(List<string>? operations = null) : IEventRepository
{
    public Event? EventToReturn { get; set; }
    public List<Event> TopEventsToReturn { get; set; } = new();
    public int GetCalls { get; private set; }
    public int GetTopCalls { get; private set; }

    public Task<List<Event>> GetAllAsync() => Task.FromResult(new List<Event>());

    public Task<Event?> GetAsync(Guid id)
    {
        GetCalls++;
        operations?.Add("get");
        return Task.FromResult(EventToReturn);
    }

    public Task<List<Event>> GetTopAsync()
    {
        GetTopCalls++;
        return Task.FromResult(TopEventsToReturn);
    }

    public Task AddAsync(Event item) => Task.CompletedTask;

    public void Delete(Event item) => operations?.Add("delete");

    public Task SaveAsync()
    {
        operations?.Add("save");
        return Task.CompletedTask;
    }
}

internal sealed class CacheServiceStub(List<string>? operations = null) : ICacheService
{
    public Dictionary<string, object> Values { get; } = new();
    public string? LastSetKey { get; private set; }
    public TimeSpan? LastExpiration { get; private set; }

    public Task<T?> GetAsync<T>(string key)
    {
        T? result = default;
        if (Values.TryGetValue(key, out var value) && value is T typedValue)
            result = typedValue;

        return Task.FromResult(result);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        LastSetKey = key;
        LastExpiration = expiration;
        Values[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        operations?.Add($"remove:{key}");
        Values.Remove(key);
        return Task.CompletedTask;
    }
}
