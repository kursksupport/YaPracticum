using EventManagementService.DTOs;
using EventManagementService.Exceptions;
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

        var eventItem = CreateTestEvent();

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

        var eventItem = CreateTestEvent();

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

        var eventItem = CreateTestEvent();

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

        var eventItem = CreateTestEvent();

        var createdEvent = eventService.Create(eventItem);

        eventService.Delete(createdEvent.Id);

        //Выполнение
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => bookingService.CreateBookingAsync(createdEvent.Id));

        //Проверка результата
        Assert.Equal("Событие не найдено", exception.Message);
    }

    //Создание брони уменьшает AvailableSeats на 1
    //Создание брони уменьшает AvailableSeats на 1
    [Fact]
    public async Task CreateBooking_Should_Decrease_AvailableSeats()
    {
        //Подготовка
        var eventService = new EventService();
        var bookingService = new BookingService(eventService);

        var createdEvent =
            eventService.Create(CreateTestEvent(5));

        var seatsBefore =
            eventService.GetById(createdEvent.Id)!
            .AvailableSeats;

        //Выполнение
        await bookingService.CreateBookingAsync(
            createdEvent.Id);

        var seatsAfter =
            eventService.GetById(createdEvent.Id)!
            .AvailableSeats;

        //Проверка результата
        Assert.Equal(
            seatsBefore - 1,
            seatsAfter);

        Assert.Equal(4, seatsAfter);
    }

    //Создание нескольких броней до лимита
    [Fact]
    public async Task CreateMultipleBookings_Until_SeatLimit_Should_Succeed()
    {
        //Подготовка
        var eventService = new EventService();

        var bookingService =
            new BookingService(eventService);

        var createdEvent =
            eventService.Create(
                CreateTestEvent(3));

        //Выполнение
        var booking1 =
            await bookingService.CreateBookingAsync(
                createdEvent.Id);

        var booking2 =
            await bookingService.CreateBookingAsync(
                createdEvent.Id);

        var booking3 =
            await bookingService.CreateBookingAsync(
                createdEvent.Id);

        var eventAfter =
            eventService.GetById(
                createdEvent.Id);

        //Проверка результата
        Assert.NotEqual(
            booking1.Id,
            booking2.Id);

        Assert.NotEqual(
            booking1.Id,
            booking3.Id);

        Assert.NotEqual(
            booking2.Id,
            booking3.Id);

        Assert.NotNull(eventAfter);

        Assert.Equal(
            0,
            eventAfter!.AvailableSeats);
    }

    //После исчерпания мест следующая бронь невозможна
    [Fact]
    public async Task CreateBooking_Should_Throw_When_No_Seats_Left()
    {
        //Подготовка
        var eventService = new EventService();

        var bookingService =
            new BookingService(eventService);

        var createdEvent =
            eventService.Create(
                CreateTestEvent(1));

        await bookingService.CreateBookingAsync(
            createdEvent.Id);

        //Выполнение
        var action = async () =>
            await bookingService.CreateBookingAsync(
                createdEvent.Id);

        //Проверка результата
        await Assert.ThrowsAsync<
            NoAvailableSeatsException>(
            action);

        var eventAfter =
            eventService.GetById(
                createdEvent.Id);

        Assert.NotNull(eventAfter);

        Assert.Equal(
            0,
            eventAfter!.AvailableSeats);
    }
    //Переход брони в Confirmed
    [Fact]
    public void Confirm_Should_Set_Status_And_ProcessedAt()
    {
        //Подготовка
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        //Выполнение
        booking.Confirm();

        //Проверка результата
        Assert.Equal(
            BookingStatus.Confirmed,
            booking.Status);

        Assert.NotNull(
            booking.ProcessedAt);
    }


    private static CreateEventDto CreateTestEvent(int totalSeats = 10)
    {
        return new CreateEventDto
        {
            Title = "Тестовое событие",
            Description = "Описание",
            StartAt = DateTime.UtcNow.AddHours(1),
            EndAt = DateTime.UtcNow.AddHours(2),
            TotalSeats = totalSeats
        };
    }
}