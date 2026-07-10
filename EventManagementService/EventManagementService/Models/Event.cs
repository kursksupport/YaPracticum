using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventManagementService.Models;
public class Event
{
    private Event()
    {
    }

    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Required]
    public int TotalSeats { get; private set; }

    [Required]
    public int AvailableSeats { get; private set; }
    [JsonIgnore]
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    public static Event Create(
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats)
    {
        if (totalSeats <= 0)
        {
            throw new ValidationException("totalSeats должно быть больше нуля");
        }

        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            StartAt = startAt,
            EndAt = endAt,
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        };
    }

    public bool TryReserveSeats(int count = 1)
    {
        if (count <= 0)
        {
            return false;
        }

        if (AvailableSeats < count)
        {
            return false;
        }

        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        if (count <= 0)
        {
            return;
        }

        AvailableSeats += count;

        if (AvailableSeats > TotalSeats)
        {
            AvailableSeats = TotalSeats;
        }
    }

    public void UpdateSeats(int totalSeats)
    {
        if (totalSeats <= 0)
        {
            throw new ValidationException(
                "TotalSeats должно быть больше нуля");
        }

        var reservedSeats = TotalSeats - AvailableSeats;

        if (totalSeats < reservedSeats)
        {
            throw new ValidationException(
                "Количество мест не может быть меньше зарезервированных");
        }

        TotalSeats = totalSeats;
        AvailableSeats = totalSeats - reservedSeats;
    }
}