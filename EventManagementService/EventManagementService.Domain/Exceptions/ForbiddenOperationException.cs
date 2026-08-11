namespace EventManagementService.Domain.Exceptions;

public class ForbiddenOperationException : DomainException
{
    public ForbiddenOperationException()
        : base("У вас нет разрешения на выполнение этой операции.")
    {
    }
}