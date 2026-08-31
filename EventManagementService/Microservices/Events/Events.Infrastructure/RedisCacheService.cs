using System.Text.Json;
using Events.Application;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Events.Infrastructure;

public sealed class RedisCacheService(
    IConnectionMultiplexer connection,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _database = connection.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось получить значение из Redis по ключу {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось сохранить значение в Redis по ключу {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось удалить значение из Redis по ключу {CacheKey}", key);
        }
    }
}
