using SalonApi.Models;
using SalonApi.Services;

namespace SalonApi.Tests;

// Each [Fact] is one test. A test follows the pattern: Arrange (set up), Act (do the thing), Assert (check the result).
public class BookingServiceTests
{
    [Fact]
    public async Task AddAsync_saves_the_booking_and_gives_it_an_id()
    {
        // Arrange: an empty database and a service that uses it
        using var db = TestDb.Create();
        var service = new BookingService(db);

        // Act: add one booking
        var saved = await service.AddAsync(new NewBookingRequest("Jane Smith", "Cut", new DateTime(2030, 1, 1, 10, 0, 0), null));

        // Assert: EF Core gave it an Id, and it is really in the database
        Assert.True(saved.Id > 0);
        Assert.Single(db.Bookings);
    }

    [Fact]
    public async Task AddAsync_rejects_an_empty_client_name()
    {
        using var db = TestDb.Create();
        var service = new BookingService(db);

        // Assert.ThrowsAsync checks that the code throws the exception we expect
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddAsync(new NewBookingRequest("   ", "Cut", new DateTime(2030, 1, 1), null)));
    }

    [Fact]
    public async Task AllAsync_returns_bookings_soonest_first()
    {
        using var db = TestDb.Create();
        var service = new BookingService(db);
        await service.AddAsync(new NewBookingRequest("Later", "Cut", new DateTime(2030, 1, 2), null));
        await service.AddAsync(new NewBookingRequest("Sooner", "Cut", new DateTime(2030, 1, 1), null));

        var all = await service.AllAsync();

        Assert.Equal(["Sooner", "Later"], all.Select(b => b.ClientName));
    }

    [Fact]
    public async Task PublicUpcomingAsync_hides_past_bookings_notes_and_surnames()
    {
        using var db = TestDb.Create();
        var service = new BookingService(db);
        var now = new DateTime(2030, 6, 15, 12, 0, 0);
        await service.AddAsync(new NewBookingRequest("Old Booking", "Cut", now.AddDays(-3), null));
        await service.AddAsync(new NewBookingRequest("Jane Smith", "Colour", now.AddDays(1), "private note"));

        var publicList = await service.PublicUpcomingAsync(now);

        var only = Assert.Single(publicList);          // the past booking is filtered out
        Assert.Equal("Jane", only.FirstName);          // surname is not shown
        Assert.Equal("Colour", only.Service);
        // PublicBookingDto has no Notes property at all, so notes can never leak to the public page.
    }

    [Fact]
    public async Task DeleteAsync_removes_the_booking_and_reports_missing_ones()
    {
        using var db = TestDb.Create();
        var service = new BookingService(db);
        var saved = await service.AddAsync(new NewBookingRequest("Jane", "Cut", new DateTime(2030, 1, 1), null));

        Assert.True(await service.DeleteAsync(saved.Id));    // found and deleted
        Assert.Empty(db.Bookings);
        Assert.False(await service.DeleteAsync(saved.Id));   // already gone
    }

    [Theory]
    [InlineData("Jane Smith", "Jane")]
    [InlineData("  Mia  ", "Mia")]
    [InlineData("", "")]
    public void FirstName_takes_the_first_word(string full, string expected)
    {
        // A [Theory] runs the same test several times with different inputs.
        Assert.Equal(expected, BookingService.FirstName(full));
    }
}
