namespace EventManagementService.Domain.Exceptions;

public class NoAvailableSeatsException : Exception
{
    public NoAvailableSeatsException()
        : base("No available seats for this event")
    {
    }
}