using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventManagementService.Models;

public class Booking
{
    private Booking()
    {
    }

    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid EventId { get; set; }
    [JsonIgnore]
    public Event Event { get; private set; } = null!;

    [Required]
    public BookingStatus Status { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public static Booking Create(Guid eventId)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
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
}