namespace Contracts;

// Public Kafka message contract. Do not add service-internal fields here.
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
