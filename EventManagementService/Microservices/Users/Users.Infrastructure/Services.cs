using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Users.Application;
using Users.Domain;

namespace Users.Infrastructure;
public sealed class UserRepository(UsersDbContext db) : IUserRepository
{
    public Task<User?> GetByLoginAsync(string login) => db.Users.SingleOrDefaultAsync(x => x.Login == login);
    public Task AddAsync(User user) => db.Users.AddAsync(user).AsTask();
    public Task SaveAsync() => db.SaveChangesAsync();
}
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    public bool Verify(string password, string hash) => Hash(password) == hash;
}
public sealed class JwtTokenService(IConfiguration config) : ITokenService
{
    public string Create(User user)
    {
        var jwt = config.GetSection("Jwt"); var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Login), new Claim(ClaimTypes.Role, user.Role.ToString()) };
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], claims, expires: DateTime.UtcNow.AddMinutes(60), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)));
    }
}
