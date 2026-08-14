namespace EventManagementService.Domain.Exceptions;

public class NoAvailableSeatsException : DomainException
{
    public NoAvailableSeatsException()
        : base("No available seats for this event")
    {
    }
}