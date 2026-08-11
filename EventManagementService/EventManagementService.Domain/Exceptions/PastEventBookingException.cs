namespace EventManagementService.Domain.Exceptions;

public class PastEventBookingException : DomainException
{
    public PastEventBookingException()
        : base("Невозможно забронировать мероприятие, которое уже началось.")
    {
    }
}