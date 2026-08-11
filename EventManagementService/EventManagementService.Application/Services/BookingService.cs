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

    public async Task<Booking> CreateBookingAsync(Guid eventId, Guid userId)
    {
        await _bookingSemaphore.WaitAsync();

        try
        {
            var eventItem = await _eventRepository.GetByIdAsync(eventId);

            if (eventItem == null)
            {
                throw new KeyNotFoundException("Событие не найдено");
            }

            var reserved = eventItem.TryReserveSeats();

            if (!reserved)
            {
                throw new NoAvailableSeatsException();
            }

            var booking = Booking.Create(eventId, userId);

            await _bookingRepository.AddAsync(booking);

            await _bookingRepository.SaveChangesAsync();

            return booking;
        }
        finally
        {
            _bookingSemaphore.Release();
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        return await _bookingRepository.GetByIdAsync(bookingId);
    }

    public async Task<List<Booking>> GetPendingBookingsAsync()
    {
        return await _bookingRepository.GetPendingAsync();
    }

    //public async Task UpdateBookingAsync(Booking booking)
    //{
    //    var existingBooking = await _bookingRepository.GetByIdAsync(booking.Id);

    //    if (existingBooking == null)
    //    {
    //        throw new KeyNotFoundException("Бронирование не найдено");
    //    }

    //    existingBooking.Status = booking.Status;
    //    existingBooking.ProcessedAt = booking.ProcessedAt;

    //    await _bookingRepository.SaveChangesAsync();
    //}
}