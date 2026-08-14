using EventManagementService.Domain.Entities;

namespace EventManagementService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByLoginAsync(string login);

    Task AddAsync(User user);

    Task SaveChangesAsync();
}