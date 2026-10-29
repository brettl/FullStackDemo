using Microsoft.EntityFrameworkCore;
using SalonApi.Models;

namespace SalonApi.Data;

/// <summary>
/// The DbContext is EF Core's "session" with the database. Each DbSet below becomes a table,
/// and you query it with LINQ (for example Bookings.Where(...).OrderBy(...)).
/// </summary>
public class SalonDbContext(DbContextOptions<SalonDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<ContentPage> ContentPages => Set<ContentPage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The content page is identified by its Key ("home" / "contact"), not by a number.
        modelBuilder.Entity<ContentPage>().HasKey(p => p.Key);
        modelBuilder.Entity<ContentPage>().Property(p => p.Key).HasMaxLength(50);

        // Usernames must be unique.
        modelBuilder.Entity<AdminUser>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<AdminUser>().Property(u => u.Username).HasMaxLength(100);

        modelBuilder.Entity<Booking>().Property(b => b.ClientName).HasMaxLength(200);
        modelBuilder.Entity<Booking>().Property(b => b.Service).HasMaxLength(200);
    }
}
