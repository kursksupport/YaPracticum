using EventApi.IntegrationTests.Collections;
using EventApi.IntegrationTests.Fixtures;
using EventManagementService.DataAccess.Repositories;
using EventManagementService.Models;

namespace EventApi.IntegrationTests;

[Collection("PostgreSql")]
public class BookingRepositoryTests
{
    private readonly PostgreSqlFixture _fixture;

    public BookingRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_Should_Save_Booking()
    {
        //Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();

        var eventRepository = new EventRepository(context);
        var bookingRepository = new BookingRepository(context);

        var eventItem = Event.Create(
            "Конференция",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            100);

        await eventRepository.AddAsync(eventItem);
        await eventRepository.SaveChangesAsync();

        var booking = Booking.Create(eventItem.Id);

        //Act
        await bookingRepository.AddAsync(booking);
        await bookingRepository.SaveChangesAsync();

        var savedBooking =
            await bookingRepository.GetByIdAsync(booking.Id);

        //Assert
        Assert.NotNull(savedBooking);
        Assert.Equal(eventItem.Id, savedBooking!.EventId);
        Assert.Equal(BookingStatus.Pending, savedBooking.Status);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        //Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();
        var repository = new BookingRepository(context);

        //Act
        var booking = await repository.GetByIdAsync(Guid.NewGuid());

        //Assert
        Assert.Null(booking);
    }

    [Fact]
    public async Task GetPendingAsync_Should_Return_Only_Pending_Bookings()
    {
        //Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();

        var eventRepository = new EventRepository(context);
        var bookingRepository = new BookingRepository(context);

        var eventItem = Event.Create(
            "Конференция",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            100);

        await eventRepository.AddAsync(eventItem);
        await eventRepository.SaveChangesAsync();

        var pending = Booking.Create(eventItem.Id);

        var confirmed = Booking.Create(eventItem.Id);
        confirmed.Confirm();

        var rejected = Booking.Create(eventItem.Id);
        rejected.Reject();

        await bookingRepository.AddAsync(pending);
        await bookingRepository.AddAsync(confirmed);
        await bookingRepository.AddAsync(rejected);

        await bookingRepository.SaveChangesAsync();

        //Act
        var result = await bookingRepository.GetPendingAsync();

        //Assert
        Assert.Single(result);
        Assert.Equal(BookingStatus.Pending, result[0].Status);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Booking_When_Exists()
    {
        //Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();

        var eventRepository = new EventRepository(context);
        var bookingRepository = new BookingRepository(context);

        var eventItem = Event.Create(
            "Конференция",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            100);

        await eventRepository.AddAsync(eventItem);
        await eventRepository.SaveChangesAsync();

        var booking = Booking.Create(eventItem.Id);

        await bookingRepository.AddAsync(booking);
        await bookingRepository.SaveChangesAsync();

        //Act
        var result = await bookingRepository.GetByIdAsync(booking.Id);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result!.Id);
        Assert.Equal(eventItem.Id, result.EventId);
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Update_Booking_Status()
    {
        //Arrange
        await _fixture.ResetDatabaseAsync();

        await using var context = _fixture.CreateContext();

        var eventRepository = new EventRepository(context);
        var bookingRepository = new BookingRepository(context);

        var eventItem = Event.Create(
            "Конференция",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            100);

        await eventRepository.AddAsync(eventItem);
        await eventRepository.SaveChangesAsync();

        var booking = Booking.Create(eventItem.Id);

        await bookingRepository.AddAsync(booking);
        await bookingRepository.SaveChangesAsync();

        //Act
        booking.Confirm();

        await bookingRepository.SaveChangesAsync();

        var updatedBooking = await bookingRepository.GetByIdAsync(booking.Id);

        //Assert
        Assert.NotNull(updatedBooking);
        Assert.Equal(BookingStatus.Confirmed, updatedBooking!.Status);
        Assert.NotNull(updatedBooking.ProcessedAt);
    }
}