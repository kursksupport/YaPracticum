using EventManagementService.Exceptions;
using EventManagementService.Models;

namespace EventManagementService.Services;

public class BookingService : IBookingService
{
    private readonly List<Booking> _bookings = new();
    private readonly IEventService _eventService;
    private readonly object _bookingLock = new();

    public BookingService(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Task<Booking> CreateBookingAsync(Guid eventId)
    {
        lock (_bookingLock)
        {
            var eventItem = _eventService.GetById(eventId);

            if (eventItem == null)
            {
                throw new KeyNotFoundException("Событие не найдено");
            }

            var reserved = eventItem.TryReserveSeats();

            if (!reserved)
            {
                throw new NoAvailableSeatsException();
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            };

            _bookings.Add(booking);

            return Task.FromResult(booking);
        }
    }

    public Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
        return Task.FromResult(booking);
    }

    public Task<List<Booking>> GetPendingBookingsAsync()
    {
        var pendingBookings = _bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .ToList();

        return Task.FromResult(pendingBookings);
    }

    public Task UpdateBookingAsync(Booking booking)
    {
        var existingBooking = _bookings.FirstOrDefault(b => b.Id == booking.Id);

        if (existingBooking == null)
        {
            throw new KeyNotFoundException("Бронирование не нейдено");
        }

        existingBooking.Status = booking.Status;
        existingBooking.ProcessedAt = booking.ProcessedAt;

        return Task.CompletedTask;
    }
}