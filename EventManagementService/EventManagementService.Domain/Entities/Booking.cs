using EventManagementService.Domain.Enums;
using EventManagementService.Domain.Exceptions;

namespace EventManagementService.Domain.Entities;

public class Booking
{
    private Booking()
    {
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Event Event { get; private set; } = null!;

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public BookingStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public static Booking Create(Guid eventId, Guid userId)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Obsolete("A booking must be associated with a user.")]
    public static Booking Create(Guid eventId)
    {
        return Create(eventId, Guid.Empty);
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            throw new DomainException("Бронь уже отменена");
        }

        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
    }
}
