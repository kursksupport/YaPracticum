using EventManagementService.DataAccess;
using EventManagementService.Exceptions;
using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    private static readonly SemaphoreSlim _bookingSemaphore = new(1, 1);

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        await _bookingSemaphore.WaitAsync();

        try
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventItem == null)
            {
                throw new KeyNotFoundException("Событие не найдено");
            }

            var reserved = eventItem.TryReserveSeats();

            if (!reserved)
            {
                throw new NoAvailableSeatsException();
            }

            var booking = Booking.Create(eventId);

            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            return booking;
        }
        finally
        {
            _bookingSemaphore.Release();
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<List<Booking>> GetPendingBookingsAsync()
    {
        return await _context.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .ToListAsync();
    }

    public async Task UpdateBookingAsync(Booking booking)
    {
        var existingBooking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == booking.Id);

        if (existingBooking == null)
        {
            throw new KeyNotFoundException("Бронирование не найдено");
        }

        existingBooking.Status = booking.Status;
        existingBooking.ProcessedAt = booking.ProcessedAt;

        await _context.SaveChangesAsync();
    }
}