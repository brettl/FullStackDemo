using Microsoft.EntityFrameworkCore;
using SalonApi.Data;
using SalonApi.Models;

namespace SalonApi.Services;

/// <summary>All the booking database work lives here, so it is easy to read and to test.</summary>
public class BookingService(SalonDbContext db)
{
    /// <summary>Add a booking. Throws ArgumentException if the name or service is empty.</summary>
    public async Task<Booking> AddAsync(NewBookingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientName)) throw new ArgumentException("Client name is required.");
        if (string.IsNullOrWhiteSpace(request.Service)) throw new ArgumentException("Service is required.");

        var booking = new Booking
        {
            ClientName = request.ClientName.Trim(),
            Service = request.Service.Trim(),
            StartsAt = request.StartsAt,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        };
        db.Bookings.Add(booking);        // mark it as "to insert"
        await db.SaveChangesAsync();     // EF Core writes the INSERT and fills in booking.Id
        return booking;
    }

    /// <summary>Every booking, soonest first (admin view).</summary>
    public Task<List<Booking>> AllAsync() =>
        db.Bookings.AsNoTracking().OrderBy(b => b.StartsAt).ToListAsync();

    /// <summary>Public list: only bookings from today onwards, first name only, soonest first.</summary>
    public async Task<List<PublicBookingDto>> PublicUpcomingAsync(DateTime now)
    {
        var from = now.Date;
        var rows = await db.Bookings.AsNoTracking()
            .Where(b => b.StartsAt >= from)
            .OrderBy(b => b.StartsAt)
            .ToListAsync();
        // Split on the client side: "Jane Smith" becomes "Jane".
        return rows.Select(b => new PublicBookingDto(FirstName(b.ClientName), b.Service, b.StartsAt)).ToList();
    }

    /// <summary>Delete by id. Returns false if there was no such booking.</summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var booking = await db.Bookings.FindAsync(id);
        if (booking is null) return false;
        db.Bookings.Remove(booking);
        await db.SaveChangesAsync();
        return true;
    }

    public static string FirstName(string clientName) =>
        clientName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? "";
}
