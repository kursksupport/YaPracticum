using EventManagementService.DataAccess;
using EventManagementService.DataAccess.Repositories;
using EventManagementService.DTOs;
using EventManagementService.Models;
using EventManagementService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagementService.Tests;

public class EventServiceTests
{

    private static ServiceProvider CreateServiceProvider()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Create_Should_Add_Event()
    {
        // Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        var eventItem = new CreateEventDto
        {
            Title = "Тест создания события",
            Description = "Тестовое описание",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };

        // Выполнение
        var createdEvent = await service.CreateAsync(eventItem);

        // Проверка результата
        Assert.NotNull(createdEvent);

        Assert.NotEqual(Guid.Empty, createdEvent.Id);

        Assert.Equal("Тест создания события", createdEvent.Title);

        var allEvents = await service.GetAllAsync(
            null,
            null,
            null,
            1,
            10);

        Assert.Single(allEvents.Items);
    }

    //получение всех событий
    // получение всех событий
    [Fact]
    public async Task GetAll_Should_Return_All_Events()
    {
        // Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Событие 1",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        });

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Событие 2",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(2),
            TotalSeats = 10
        });

        // Выполнение
        var result = await service.GetAllAsync(
            null,
            null,
            null,
            1,
            10);

        // Проверка результата
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
    }

    //получение события по ID
    [Fact]
    public async Task GetById_Should_Return_Event()
    {
        // Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        var eventItem = new CreateEventDto
        {
            Title = "получение события по Id",
            Description = "Тестовое описание",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(2),
            TotalSeats = 10
        };

        var createdEvent = await service.CreateAsync(eventItem);

        // Выполнение
        var result = await service.GetByIdAsync(createdEvent.Id);

        // Проверка результата
        Assert.NotNull(result);

        Assert.Equal(createdEvent.Id, result.Id);

        Assert.Equal("получение события по Id", result.Title);
    }

    //попытка получить событие с несуществующим ID
    [Fact]
    public async Task GetById_Should_Return_Null_When_Event_Not_Found()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        //Выполнение
        var result = await service.GetByIdAsync(Guid.NewGuid());

        //Проверка результата
        Assert.Null(result);
    }

    //обновление существующего события
    [Fact]
    public async Task Update_Should_Modify_Existing_Event()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        var eventItem = new CreateEventDto
        {
            Title = "Первоначальное значение",
            Description = "Первоначальное олписание",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };

        var createdEvent = await service.CreateAsync(eventItem);

        var updatedEvent = Event.Create("Новое значение",
                                        "Новое описание",
                                        DateTime.Now.AddDays(1),
                                        DateTime.Now.AddDays(1).AddHours(2),
                                        100);

        //Выполнение
        var result = await service.UpdateAsync(createdEvent.Id, updatedEvent);

        var savedEvent = await service.GetByIdAsync(createdEvent.Id);

        //Проверка результата
        Assert.True(result);

        Assert.NotNull(savedEvent);

        Assert.Equal("Новое значение", savedEvent.Title);

        Assert.Equal("Новое описание", savedEvent.Description);
    }

    //попытка обновить событие с несуществующим ID
    [Fact]
    public async Task Update_Should_Return_False_When_Event_Not_Found()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        var updatedEvent = Event.Create("попытка обновить событие с несуществующим ID",
                                        "Тестовое описание",
                                        DateTime.Now.AddDays(1),
                                        DateTime.Now.AddDays(1).AddHours(2),
                                        100);

        //Выполнение
        var result = await service.UpdateAsync(Guid.NewGuid(), updatedEvent);

        //Проверка результата
        Assert.False(result);
    }

    //удаление существующего события
    [Fact]
    public async Task Delete_Should_Remove_Existing_Event()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        var eventItem = new CreateEventDto
        {
            Title = "удаление существующего события",
            Description = "Тестовое описани",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        };

        var createdEvent = await service.CreateAsync(eventItem);

        //Выполнение
        var result = await service.DeleteAsync(createdEvent.Id);

        var deletedEvent = await service.GetByIdAsync(createdEvent.Id);

        //Проверка результата
        Assert.True(result);

        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task Delete_Should_Return_False_When_Event_Not_Found()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        //Выполнение
        var result = await service.DeleteAsync(Guid.NewGuid());

        //Проверка результата
        Assert.False(result);
    }

    //фильтрация по названию
    [Fact]
    public async Task GetAll_Should_Filter_By_Title()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Тест событие Фильтр",
            Description = "Тестовое описание",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 10
        });

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Событие 2",
            Description = "Тестовое описание 2",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(2),
            TotalSeats = 10
        });

        //Выполнение
        var result = await service.GetAllAsync("Тест", null, null, 1, 10);

        //Проверка результата
        Assert.Single(result.Items);

        Assert.Equal("Тест событие Фильтр", result.Items[0].Title);
    }

    //фильтрация по датам
    [Fact]
    public async Task GetAll_Should_Filter_By_Date_Range()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Первое событие",
            StartAt = new DateTime(2026, 1, 1),
            EndAt = new DateTime(2026, 1, 2),
            TotalSeats = 10
        });

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Второе событие",
            StartAt = new DateTime(2026, 6, 1),
            EndAt = new DateTime(2026, 6, 2),
            TotalSeats = 10
        });

        //Выполнение
        var result = await service.GetAllAsync(
            null,
            new DateTime(2026, 5, 1),
            new DateTime(2026, 12, 31),
            1,
            10);

        //Проверка результата
        Assert.Single(result.Items);

        Assert.Equal("Второе событие", result.Items[0].Title);
    }

    //пагинация событий
    [Fact]
    public async Task GetAll_Should_Return_Correct_Page()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        for (int i = 1; i <= 15; i++)
        {
            await service.CreateAsync(new CreateEventDto
            {
                Title = $"Событие {i}",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1),
                TotalSeats = 10
            });
        }

        //Выполнение
        var result = await service.GetAllAsync(null, null, null, 2, 5);

        //Проверка результата
        Assert.Equal(5, result.Items.Count);
        Assert.Equal("Событие 6", result.Items[0].Title);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
    }

    //комбинированная фильтрация
    [Fact]
    public async Task GetAll_Should_Apply_Combined_Filters()
    {
        //Подготовка
        using var provider = CreateServiceProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IEventService>();

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Тестовое событие 1",
            StartAt = new DateTime(2026, 6, 1),
            EndAt = new DateTime(2026, 6, 2),
            TotalSeats = 10
        });

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Тестовое событие 2",
            StartAt = new DateTime(2025, 6, 1),
            EndAt = new DateTime(2025, 6, 2),
            TotalSeats = 10
        });

        await service.CreateAsync(new CreateEventDto
        {
            Title = "Событие",
            StartAt = new DateTime(2026, 6, 1),
            EndAt = new DateTime(2026, 6, 2),
            TotalSeats = 10
        });

        //Выполнение
        var result = await service.GetAllAsync(
            "Тест",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31),
            1,
            10);

        //Проверка результата
        Assert.Single(result.Items);

        Assert.Equal("Тестовое событие 1", result.Items[0].Title);
    }
}