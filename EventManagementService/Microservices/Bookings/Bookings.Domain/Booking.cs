namespace Bookings.Domain;
public enum BookingStatus { Pending, Confirmed, Cancelled }
public class Booking
{
    private Booking() { }
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public static Booking Create(Guid eventId, Guid userId) => new() { Id = Guid.NewGuid(), EventId = eventId, UserId = userId, Status = BookingStatus.Pending, CreatedAt = DateTime.UtcNow };
    public void Confirm() { if (Status == BookingStatus.Pending) Status = BookingStatus.Confirmed; }
    public void Cancel() { if (Status == BookingStatus.Cancelled) throw new InvalidOperationException("Бронь уже отменена"); Status = BookingStatus.Cancelled; }
}
