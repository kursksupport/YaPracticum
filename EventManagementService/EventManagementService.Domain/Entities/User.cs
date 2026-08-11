using EventManagementService.Domain.Enums;

namespace EventManagementService.Domain.Entities;

public class User
{
    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Login { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    public static User Create(
        string login,
        string passwordHash,
        UserRole role = UserRole.User)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            PasswordHash = passwordHash,
            Role = role
        };
    }
}