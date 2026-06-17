using EventManagementService.Models;
using EventManagementService.Services;
using System.Threading;

namespace EventManagementService.BackgroundServices;

public class BookingProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingProcessingService> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    public BookingProcessingService(
        IServiceProvider serviceProvider,
        ILogger<BookingProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();

            var bookingService =
                scope.ServiceProvider.GetRequiredService<IBookingService>();

            var eventService =
                scope.ServiceProvider.GetRequiredService<IEventService>();

            var pendingBookings =
                (await bookingService.GetPendingBookingsAsync())
                .ToList();

            var tasks = pendingBookings
                .Select(b => ProcessBookingAsync(
                    b,
                    bookingService,
                    eventService,
                    stoppingToken));

            await Task.WhenAll(tasks);

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessBookingAsync(
    Booking booking,
    IBookingService bookingService,
    IEventService eventService,
    CancellationToken stoppingToken)
    {
        _logger.LogInformation(
    "Обработка бронирования {BookingId}",
    booking.Id);

        try
        {
            await Task.Delay(2000, stoppingToken);

            await _processingSemaphore.WaitAsync(stoppingToken);

            try
            {
                var eventItem =
                    eventService.GetById(booking.EventId);

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
            finally
            {
                _processingSemaphore.Release();
            }
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

            var eventItem =
                eventService.GetById(booking.EventId);

            eventItem?.ReleaseSeats();

            await bookingService.UpdateBookingAsync(booking);
        }
    }
}