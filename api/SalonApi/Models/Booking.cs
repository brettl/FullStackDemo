namespace SalonApi.Models;

/// <summary>One appointment in the salon diary. EF Core turns this class into the "Bookings" table.</summary>
public class Booking
{
    public int Id { get; set; }                       // primary key (EF finds "Id" by convention)
    public string ClientName { get; set; } = "";      // e.g. "Jane Smith"
    public string Service { get; set; } = "";         // e.g. "Cut and blow dry"
    public DateTime StartsAt { get; set; }            // date and time of the appointment
    public string? Notes { get; set; }                // private notes, never shown on the public page
}
