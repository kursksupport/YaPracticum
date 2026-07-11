using EventManagementService.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests.Fixtures;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .WithDatabase("eventapi_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new AppDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }
}