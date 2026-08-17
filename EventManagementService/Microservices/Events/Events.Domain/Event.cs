namespace Events.Domain;
public class Event
{
    private Event() { }
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }
    public static Event Create(string title, string? description, DateTime startAt, DateTime endAt, int seats) => new() { Id = Guid.NewGuid(), Title = title, Description = description, StartAt = startAt, EndAt = endAt, TotalSeats = seats, AvailableSeats = seats };
    public void Update(string title, string? description, DateTime startAt, DateTime endAt, int seats) { Title = title; Description = description; StartAt = startAt; EndAt = endAt; AvailableSeats += seats - TotalSeats; TotalSeats = seats; }
}
