using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.DataAccess.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }


    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id);
    }


    public async Task<List<Booking>> GetPendingAsync()
    {
        return await _context.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .ToListAsync();
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}