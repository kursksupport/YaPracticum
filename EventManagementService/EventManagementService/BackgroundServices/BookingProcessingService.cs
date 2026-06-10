using EventManagementService.Models;
using EventManagementService.Services;

namespace EventManagementService.BackgroundServices;

public class BookingProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingProcessingService> _logger;

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

            var pendingBookings =
                await bookingService.GetPendingBookingsAsync();

            foreach (var booking in pendingBookings)
            {
                _logger.LogInformation(
                    "Обработка бронирования {BookingId}",
                    booking.Id);

                await Task.Delay(2000, stoppingToken);

                booking.Status = BookingStatus.Confirmed;
                booking.ProcessedAt = DateTime.UtcNow;

                await bookingService.UpdateBookingAsync(booking);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}