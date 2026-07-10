using EventManagementService.Models;
using EventManagementService.Services;

namespace EventManagementService.BackgroundServices;

public class BookingProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingProcessingService> _logger;

    public BookingProcessingService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingProcessingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var bookingService =
                    scope.ServiceProvider
                        .GetRequiredService<IBookingService>();

                var pendingBookings =
                    await bookingService.GetPendingBookingsAsync();

                var tasks = pendingBookings
                    .Select(b => ProcessBookingAsync(
                        b,
                        stoppingToken));

                await Task.WhenAll(tasks);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessBookingAsync(
        Booking booking,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Обработка бронирования {BookingId}",
            booking.Id);

        try
        {
            await Task.Delay(2000, stoppingToken);

            using var scope = _scopeFactory.CreateScope();

            var bookingService =
                scope.ServiceProvider
                    .GetRequiredService<IBookingService>();

            var eventService =
                scope.ServiceProvider
                    .GetRequiredService<IEventService>();

            var eventItem =
                await eventService.GetByIdAsync(
                    booking.EventId);

            if (eventItem == null)
            {
                booking.Reject();

                await bookingService.UpdateBookingAsync(booking);

                _logger.LogWarning(
                    "Событие удалено для брони {BookingId}",
                    booking.Id);

                return;
            }

            booking.Confirm();

            await bookingService.UpdateBookingAsync(booking);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка обработки брони {BookingId}",
                booking.Id);

            booking.Reject();

            using var scope = _scopeFactory.CreateScope();

            var bookingService =
                scope.ServiceProvider
                    .GetRequiredService<IBookingService>();

            var eventService =
                scope.ServiceProvider
                    .GetRequiredService<IEventService>();

            var eventItem =
                await eventService.GetByIdAsync(
                    booking.EventId);

            eventItem?.ReleaseSeats();

            await bookingService.UpdateBookingAsync(booking);
        }
    }
}