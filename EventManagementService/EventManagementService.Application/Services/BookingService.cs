using EventManagementService.Application.Interfaces;
using EventManagementService.Domain.Entities;
using EventManagementService.Domain.Exceptions;

namespace EventManagementService.Application.Services;

public class BookingService : IBookingService
{
    private static readonly SemaphoreSlim _bookingSemaphore = new(1, 1);

    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;

    public BookingService(
        IEventRepository eventRepository,
        IBookingRepository bookingRepository)
    {
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<Booking> CreateBookingAsync(
        Guid eventId,
        Guid userId)
    {
        await _bookingSemaphore.WaitAsync();

        try
        {
            var eventItem =
                await _eventRepository.GetByIdAsync(eventId);

            if (eventItem == null)
            {
                throw new KeyNotFoundException(
                    "Событие не найдено");
            }

            if (eventItem.StartAt <= DateTime.UtcNow)
            {
                throw new PastEventBookingException();
            }

            var activeBookings =
                await _bookingRepository
                    .CountActiveByUserIdAsync(userId);

            if (activeBookings >= 10)
            {
                throw new BookingLimitExceededException();
            }

            var reserved = eventItem.TryReserveSeats();

            if (!reserved)
            {
                throw new NoAvailableSeatsException();
            }

            var booking = Booking.Create(
                eventId,
                userId);

            await _bookingRepository.AddAsync(booking);

            await _bookingRepository.SaveChangesAsync();

            return booking;
        }
        finally
        {
            _bookingSemaphore.Release();
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(
        Guid bookingId)
    {
        return await _bookingRepository
            .GetByIdAsync(bookingId);
    }

    public async Task CancelBookingAsync(
        Guid bookingId,
        Guid userId,
        EventManagementService.Domain.Enums.UserRole userRole)
    {
        await _bookingSemaphore.WaitAsync();

        try
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                throw new KeyNotFoundException("Бронь не найдена");
            }

            if (booking.UserId != userId &&
                userRole != EventManagementService.Domain.Enums.UserRole.Admin)
            {
                throw new ForbiddenOperationException();
            }

            var eventItem = await _eventRepository.GetByIdAsync(booking.EventId);

            booking.Cancel();
            eventItem?.ReleaseSeats();

            await _bookingRepository.SaveChangesAsync();
        }
        finally
        {
            _bookingSemaphore.Release();
        }
    }

    public async Task<List<Booking>> GetPendingBookingsAsync()
    {
        return await _bookingRepository
            .GetPendingAsync();
    }
}
