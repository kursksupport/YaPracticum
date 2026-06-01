using EventManagementService.Models;
using EventManagementService.Services;

namespace EventManagementService.Tests;

public class BookingServiceTests
{
    //создание брони для существующего события
    [Fact]
    public async Task CreateBooking_Should_Return_Pending_Booking_For_Existing_Event()
    {
        //Подготовка
        var eventService = new EventService();
        var bookingService = new BookingService(eventService);

        var eventItem = new Event
        {
            Title = "Тестовое событие",
            Description = "Описание",
            StartAt = DateTime.UtcNow.AddHours(1),
            EndAt = DateTime.UtcNow.AddHours(2)
        };

        var createdEvent = eventService.Create(eventItem);

        //Выполнение
        var booking = await bookingService.CreateBookingAsync(createdEvent.Id);

        //Проверка результата
        Assert.NotNull(booking);
        Assert.Equal(createdEvent.Id, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(Guid.Empty, booking.Id);
    }

    //Создание нескольких броней для одного события
    [Fact]
    public async Task CreateMultipleBookings_Should_Generate_Unique_Ids()
    {
        //Подготовка
        var eventService = new EventService();
        var bookingService = new BookingService(eventService);

        var eventItem = new Event
        {
            Title = "Тестовое событие",
            Description = "Описание",
            StartAt = DateTime.UtcNow.AddHours(1),
            EndAt = DateTime.UtcNow.AddHours(2)
        };

        var createdEvent = eventService.Create(eventItem);

        //Выполнение
        var booking1 = await bookingService.CreateBookingAsync(createdEvent.Id);
        var booking2 = await bookingService.CreateBookingAsync(createdEvent.Id);
        var booking3 = await bookingService.CreateBookingAsync(createdEvent.Id);

        //Проверка результата
        Assert.NotEqual(booking1.Id, booking2.Id);
        Assert.NotEqual(booking1.Id, booking3.Id);
        Assert.NotEqual(booking2.Id, booking3.Id);
    }

    //получение брони по Id
    [Fact]
    public async Task GetBookingById_Should_Return_Correct_Booking()
    {
        //Подготовка
        var eventService = new EventService();
        var bookingService = new BookingService(eventService);

        var eventItem = new Event
        {
            Title = "Тестовое событие",
            Description = "Описание",
            StartAt = DateTime.UtcNow.AddHours(1),
            EndAt = DateTime.UtcNow.AddHours(2)
        };

        var createdEvent = eventService.Create(eventItem);

        var createdBooking = await bookingService.CreateBookingAsync(createdEvent.Id);

        //Выполнение
        var result = await bookingService.GetBookingByIdAsync(createdBooking.Id);

        //Проверка результата
        Assert.NotNull(result);

        Assert.Equal(createdBooking.Id, result!.Id);

        Assert.Equal(createdEvent.Id, result.EventId);

        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    //создание брони для несуществующего события
    [Fact]
    public async Task CreateBooking_Should_Throw_When_Event_Not_Found()
    {
        //Подготовка
        var eventService = new EventService();
        var bookingService = new BookingService(eventService);

        //Выполнение
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => bookingService.CreateBookingAsync(Guid.NewGuid()));

        //Проверка результата
        Assert.Equal("Событие не найдено", exception.Message);
    }

    //получение брони по несуществующему Id
    [Fact]
    public async Task GetBookingById_Should_Return_Null_When_Booking_Not_Found()
    {
        var eventService = new EventService();
        var bookingService = new BookingService(eventService);

        var result = await bookingService.GetBookingByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    //создание брони для удалённого события;
    [Fact]
    public async Task CreateBooking_Should_Throw_When_Event_Was_Deleted()
    {
        //Подготовка
        var eventService = new EventService();
        var bookingService = new BookingService(eventService);

        var eventItem = new Event
        {
            Title = "Тестовое событие",
            Description = "Описание",
            StartAt = DateTime.UtcNow.AddHours(1),
            EndAt = DateTime.UtcNow.AddHours(2)
        };

        var createdEvent = eventService.Create(eventItem);

        eventService.Delete(createdEvent.Id);

        //Выполнение
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => bookingService.CreateBookingAsync(createdEvent.Id));

        //Проверка результата
        Assert.Equal("Событие не найдено", exception.Message);
    }
}