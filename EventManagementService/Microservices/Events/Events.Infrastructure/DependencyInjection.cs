using Events.Application;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
