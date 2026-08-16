namespace EventManagementService.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string login, string role);
}