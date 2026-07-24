using EventApi.IntegrationTests.Collections;
using EventApi.IntegrationTests.Fixtures;
using EventManagementService.DataAccess.Repositories;
using EventManagementService.Domain.Entities;
using EventManagementService.Domain.Enums;

namespace EventApi.IntegrationTests;

[Collection("PostgreSql")]
public class EventRepositoryTests
{
    private readonly PostgreSqlFixture _fixture;

    public EventRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_Should_Save_Event()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        var eventItem = Event.Create(
            "Тестовое событие",
            "Описание",
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            100);

        // Act
        await repository.AddAsync(eventItem);
        await repository.SaveChangesAsync();

        var savedEvent = await repository.GetByIdAsync(eventItem.Id);

        // Assert
        Assert.NotNull(savedEvent);
        Assert.Equal(eventItem.Id, savedEvent!.Id);
        Assert.Equal("Тестовое событие", savedEvent.Title);
        Assert.Equal(100, savedEvent.TotalSeats);
        Assert.Equal(100, savedEvent.AvailableSeats);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Event_When_Exists()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        var eventItem = Event.Create(
            "Концерт",
            "Тест",
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            50);

        await repository.AddAsync(eventItem);
        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(eventItem.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventItem.Id, result!.Id);
        Assert.Equal("Концерт", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Events()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        await repository.AddAsync(Event.Create(
            "Событие 1",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            10));

        await repository.AddAsync(Event.Create(
            "Событие 2",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            20));

        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(
            null,
            null,
            null,
            1,
            10);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter_By_Title()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        await repository.AddAsync(Event.Create(
            "Рок концерт",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            10));

        await repository.AddAsync(Event.Create(
            "Футбольный матч",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            20));

        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(
            "Рок",
            null,
            null,
            1,
            10);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Рок концерт", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter_By_From_Date()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        var oldDate = DateTime.UtcNow.AddDays(-10);
        var futureDate = DateTime.UtcNow.AddDays(10);

        await repository.AddAsync(Event.Create(
            "Старое событие",
            null,
            oldDate,
            oldDate.AddHours(1),
            10));

        await repository.AddAsync(Event.Create(
            "Будущее событие",
            null,
            futureDate,
            futureDate.AddHours(1),
            20));

        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(
            null,
            DateTime.UtcNow,
            null,
            1,
            10);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Будущее событие", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter_By_To_Date()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        var soon = DateTime.UtcNow.AddDays(1);
        var later = DateTime.UtcNow.AddDays(20);

        await repository.AddAsync(Event.Create(
            "Ближайшее событие",
            null,
            soon,
            soon.AddHours(1),
            10));

        await repository.AddAsync(Event.Create(
            "Позднее событие",
            null,
            later,
            later.AddHours(1),
            20));

        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(
            null,
            null,
            DateTime.UtcNow.AddDays(5),
            1,
            10);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Ближайшее событие", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter_By_All_Parameters()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        var targetDate = DateTime.UtcNow.AddDays(5);

        await repository.AddAsync(Event.Create(
            "Музыкальный фестиваль",
            null,
            targetDate,
            targetDate.AddHours(3),
            100));

        await repository.AddAsync(Event.Create(
            "Музыкальный фестиваль прошедший",
            null,
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(-5).AddHours(3),
            100));

        await repository.AddAsync(Event.Create(
            "Спортивное событие",
            null,
            targetDate,
            targetDate.AddHours(3),
            100));

        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(
            "Музыкальный",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(10),
            1,
            10);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Музыкальный фестиваль", result.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Correct_Page()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        for (int i = 1; i <= 5; i++)
        {
            await repository.AddAsync(Event.Create(
                $"Событие {i}",
                null,
                DateTime.UtcNow.AddDays(i),
                DateTime.UtcNow.AddDays(i).AddHours(1),
                10));
        }

        await repository.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync(
            null,
            null,
            null,
            2,
            2);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Event()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new EventRepository(context);

        var eventItem = Event.Create(
            "Событие для удаления",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            50);

        await repository.AddAsync(eventItem);
        await repository.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(eventItem);
        await repository.SaveChangesAsync();

        var result = await repository.GetByIdAsync(eventItem.Id);

        // Assert
        Assert.Null(result);
    }
}