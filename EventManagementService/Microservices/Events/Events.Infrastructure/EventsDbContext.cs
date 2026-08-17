using Events.Domain; using Microsoft.EntityFrameworkCore;
namespace Events.Infrastructure;
public sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options) { public DbSet<Event> Events => Set<Event>(); protected override void OnModelCreating(ModelBuilder b) { b.Entity<Event>(x => { x.ToTable("events"); x.HasKey(e => e.Id); x.Property(e => e.Title).IsRequired().HasMaxLength(200); }); } }
