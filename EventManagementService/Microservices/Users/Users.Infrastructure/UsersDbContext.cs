using Microsoft.EntityFrameworkCore;
using Users.Domain;

namespace Users.Infrastructure;
public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b => { b.ToTable("users"); b.HasKey(x => x.Id); b.Property(x => x.Login).IsRequired(); b.HasIndex(x => x.Login).IsUnique(); b.Property(x => x.Role).HasConversion<string>(); });
    }
}
