using Users.Domain;

namespace Users.Application;

public record RegisterRequest(string Login, string Password, string? Role);
public record LoginRequest(string Login, string Password);
public interface IUserRepository { Task<User?> GetByLoginAsync(string login); Task AddAsync(User user); Task SaveAsync(); }
public interface IPasswordHasher { string Hash(string password); bool Verify(string password, string hash); }
public interface ITokenService { string Create(User user); }
public interface IUserService { Task RegisterAsync(RegisterRequest request); Task<string> LoginAsync(LoginRequest request); }

public sealed class UserService(IUserRepository users, IPasswordHasher passwords, ITokenService tokens) : IUserService
{
    public async Task RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Логин и пароль обязательны");
        if (await users.GetByLoginAsync(request.Login) is not null) throw new ArgumentException("Пользователь уже существует");
        var role = Enum.TryParse<UserRole>(request.Role, true, out var value) ? value : UserRole.User;
        await users.AddAsync(User.Create(request.Login, passwords.Hash(request.Password), role));
        await users.SaveAsync();
    }
    public async Task<string> LoginAsync(LoginRequest request)
    {
        var user = await users.GetByLoginAsync(request.Login);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash)) throw new ArgumentException("Неверный логин или пароль");
        return tokens.Create(user);
    }
}
