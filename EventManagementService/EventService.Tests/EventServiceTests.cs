using EventManagementService.Models;
using EventManagementService.Services;

namespace EventManagementService.Tests;

public class EventServiceTests
{
    [Fact]
    public void Create_Should_Add_Event()
    {
        //Подготовка
        var service = new EventService();

        var eventItem = new Event
        {
            Title = "Тест создания события",
            Description = "Тестовое описание",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1)
        };

        //Выполнение
        var createdEvent = service.Create(eventItem);

        //Проверка результата
        Assert.NotNull(createdEvent);

        Assert.Equal(1, createdEvent.Id);

        Assert.Equal("Тест создания события", createdEvent.Title);

        var allEvents = service.GetAll(null, null, null, 1, 10);

        Assert.Single(allEvents.Items);
    }

    [Fact]
    public void GetById_Should_Return_Event()
    {
        //Подготовка
        var service = new EventService();

        var eventItem = new Event
        {
            Title = "получение события по Id",
            Description = "Тестовое описание",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(2)
        };

        var createdEvent = service.Create(eventItem);

        //Выполнение
        var result = service.GetById(createdEvent.Id);

        //Проверка результата
        Assert.NotNull(result);

        Assert.Equal(createdEvent.Id, result.Id);

        Assert.Equal("получение события по Id", result.Title);
    }

    [Fact]
    public void GetById_Should_Return_Null_When_Event_Not_Found()
    {
        //Подготовка
        var service = new EventService();

        //Выполнение
        var result = service.GetById(123);

        //Проверка результата
        Assert.Null(result);
    }

    [Fact]
    public void Update_Should_Modify_Existing_Event()
    {
        //Подготовка
        var service = new EventService();

        var eventItem = new Event
        {
            Title = "Первоначальное значение",
            Description = "Первоначальное олписание",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1)
        };

        var createdEvent = service.Create(eventItem);

        var updatedEvent = new Event
        {
            Title = "Новое значение",
            Description = "Новое описание",
            StartAt = DateTime.Now.AddDays(1),
            EndAt = DateTime.Now.AddDays(1).AddHours(2)
        };

        //Выполнение
        var result = service.Update(createdEvent.Id, updatedEvent);

        var savedEvent = service.GetById(createdEvent.Id);

        //Проверка результата
        Assert.True(result);

        Assert.NotNull(savedEvent);

        Assert.Equal("Новое значение", savedEvent.Title);

        Assert.Equal("Новое описание", savedEvent.Description);
    }

    [Fact]
    public void Update_Should_Return_False_When_Event_Not_Found()
    {
        //Подготовка
        var service = new EventService();

        var updatedEvent = new Event
        {
            Title = "попытка обновить событие с несуществующим ID",
            Description = "Тестовое описание",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1)
        };

        //Выполнение
        var result = service.Update(111, updatedEvent);

        //Проверка результата
        Assert.False(result);
    }

    [Fact]
    public void Delete_Should_Remove_Existing_Event()
    {
        //Подготовка
        var service = new EventService();

        var eventItem = new Event
        {
            Title = "удаление существующего события",
            Description = "Тестовое описани",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1)
        };

        var createdEvent = service.Create(eventItem);

        //Выполнение
        var result = service.Delete(createdEvent.Id);

        var deletedEvent = service.GetById(createdEvent.Id);

        //Проверка результата
        Assert.True(result);

        Assert.Null(deletedEvent);
    }
}