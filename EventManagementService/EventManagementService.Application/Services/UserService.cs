using EventManagementService.Application.Interfaces;
using EventManagementService.Domain.Entities;
using EventManagementService.Domain.Enums;

namespace EventManagementService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task RegisterAsync(
        string login,
        string password,
        string? role)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ArgumentException(
                "Логин не может быть пустым");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Пароль не может быть пустым");
        }

        var existingUser =
            await _userRepository.GetByLoginAsync(login);

        if (existingUser != null)
        {
            throw new ArgumentException(
                "Пользователь с таким логином уже существует");
        }

        var userRole = UserRole.User;

        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!Enum.TryParse<UserRole>(
                    role,
                    ignoreCase: true,
                    out userRole))
            {
                throw new ArgumentException(
                    "Недопустимая роль пользователя");
            }
        }

        var passwordHash = _passwordHasher.Hash(password);

        var user = User.Create(
            login,
            passwordHash,
            userRole);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<string> LoginAsync(
        string login,
        string password)
    {
        var user =
            await _userRepository.GetByLoginAsync(login);

        if (user == null)
        {
            throw new ArgumentException(
                "Неверный логин или пароль");
        }

        var passwordValid =
            _passwordHasher.Verify(
                password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new ArgumentException(
                "Неверный логин или пароль");
        }

        return _jwtTokenService.GenerateToken(
            user.Id,
            user.Login,
            user.Role.ToString());
    }
}