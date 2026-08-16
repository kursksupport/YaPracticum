namespace EventManagementService.Domain.Exceptions;

public class BookingLimitExceededException : DomainException
{
    public BookingLimitExceededException()
        : base("Превышено максимальное количество активных бронирований.")
    {
    }
}