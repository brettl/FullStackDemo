using Microsoft.EntityFrameworkCore;
using SalonApi.Models;
using SalonApi.Services;

namespace SalonApi.Data;

/// <summary>Creates the tables and fills in starter data the first time the app runs. Everything here is made up.</summary>
public static class DbSeeder
{
    public static async Task InitialiseAsync(SalonDbContext db, string adminUser, string adminPassword, DateTime today)
    {
        await db.Database.EnsureCreatedAsync();   // creates the database and tables if they do not exist

        if (!await db.AdminUsers.AnyAsync())
            db.AdminUsers.Add(new AdminUser { Username = adminUser, PasswordHash = PasswordHasher.Hash(adminPassword) });

        if (!await db.ContentPages.AnyAsync())
        {
            db.ContentPages.AddRange(
                new ContentPage
                {
                    Key = "home",
                    Title = "Welcome to Sample Hair Studio",
                    Body = "Cuts, colour and styling in a relaxed, friendly studio.\n\nWalk-ins are welcome, but booking ahead guarantees your favourite time.",
                },
                new ContentPage
                {
                    Key = "contact",
                    Title = "Find us",
                    Body = "12 Example Street, Sampletown\n\nPhone: 000 000 0000\n\nOpen Tuesday to Saturday, 9am to 5pm.",
                });
        }

        if (!await db.Bookings.AnyAsync())
        {
            var d = today.Date;
            db.Bookings.AddRange(
                new Booking { ClientName = "Jane Smith", Service = "Cut and blow dry", StartsAt = d.AddDays(1).AddHours(10), Notes = "Prefers a quiet chair" },
                new Booking { ClientName = "Mia Jones", Service = "Full colour", StartsAt = d.AddDays(1).AddHours(13) },
                new Booking { ClientName = "Ava Brown", Service = "Trim", StartsAt = d.AddDays(2).AddHours(9).AddMinutes(30) });
        }

        await db.SaveChangesAsync();
    }
}
