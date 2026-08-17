namespace Contracts;

public sealed record BookingConfirmed(Guid BookingId, Guid EventId, Guid UserId, DateTime ConfirmedAt);

public static class RabbitMqNames
{
    public const string BookingsExchange = "bookings.exchange";
    public const string BookingConfirmedRoutingKey = "booking.confirmed";
    public const string EventsQueue = "events.booking-confirmed";
}
