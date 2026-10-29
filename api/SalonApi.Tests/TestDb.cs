using Microsoft.EntityFrameworkCore;
using SalonApi.Data;

namespace SalonApi.Tests;

/// <summary>
/// Makes a brand-new, empty, in-memory database for each test. Every test gets its own database
/// (the Guid makes the name unique), so tests cannot affect each other.
/// </summary>
public static class TestDb
{
    public static SalonDbContext Create()
    {
        var options = new DbContextOptionsBuilder<SalonDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SalonDbContext(options);
    }
}
