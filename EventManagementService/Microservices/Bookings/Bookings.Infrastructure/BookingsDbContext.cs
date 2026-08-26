using Bookings.Domain; using Microsoft.EntityFrameworkCore;
namespace Bookings.Infrastructure;
public sealed class BookingsDbContext(DbContextOptions<BookingsDbContext> options) : DbContext(options) { public DbSet<Booking> Bookings => Set<Booking>(); protected override void OnModelCreating(ModelBuilder b) { b.Entity<Booking>(x => { x.ToTable("bookings"); x.HasKey(e => e.Id); x.Property(e => e.Status).HasConversion<string>(); }); } }
