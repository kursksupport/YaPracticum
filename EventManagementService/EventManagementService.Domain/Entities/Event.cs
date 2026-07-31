using EventManagementService.Domain.Exceptions;

namespace EventManagementService.Domain.Entities;
public class Event
{
    private Event()
    {
    }

    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public int TotalSeats { get; private set; }

    public int AvailableSeats { get; private set; }

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
            throw new DomainException("totalSeats должно быть больше нуля");
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
            throw new DomainException("TotalSeats должно быть больше нуля");
        }

        var reservedSeats = TotalSeats - AvailableSeats;

        if (totalSeats < reservedSeats)
        {
            throw new DomainException("Количество мест не может быть меньше зарезервированных");
        }

        TotalSeats = totalSeats;
        AvailableSeats = totalSeats - reservedSeats;
    }
}