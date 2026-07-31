using EventManagementService.Application.Interfaces;
using EventManagementService.Domain.Entities;

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
                var bookingRepository =
                    scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var pendingBookings =
                    await bookingRepository.GetPendingAsync();

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

            var bookingRepository =
                scope.ServiceProvider
                    .GetRequiredService<IBookingRepository>();

            var eventRepository =
                scope.ServiceProvider
                    .GetRequiredService<IEventRepository>();

            var eventItem =
                await eventRepository.GetByIdAsync(
                    booking.EventId);

            if (eventItem == null)
            {
                booking.Reject();

                await bookingRepository.SaveChangesAsync();

                _logger.LogWarning(
                    "Событие удалено для брони {BookingId}",
                    booking.Id);

                return;
            }

            booking.Confirm();

            await bookingRepository.SaveChangesAsync();
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

            var bookingRepository =
                scope.ServiceProvider
                    .GetRequiredService<IBookingRepository>();

            var eventRepository =
                scope.ServiceProvider
                    .GetRequiredService<IEventRepository>();

            var eventItem =
                await eventRepository.GetByIdAsync(
                    booking.EventId);

            eventItem?.ReleaseSeats();

            await bookingRepository.SaveChangesAsync();
        }
    }
}