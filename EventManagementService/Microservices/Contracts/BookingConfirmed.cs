namespace Contracts;

public sealed record BookingConfirmed(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    int SeatsCount,
    DateTime ConfirmedAt);

public static class KafkaTopics
{
    public const string BookingConfirmed = "booking-confirmed";
}
