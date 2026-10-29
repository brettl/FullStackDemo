using SalonApi.Data;
using SalonApi.Models;
using SalonApi.Services;

namespace SalonApi.Tests;

public class ContentAndAuthTests
{
    private static readonly JwtSettings Jwt = new() { Key = "a-test-key-that-is-at-least-32-chars-long" };

    [Fact]
    public async Task Seeder_creates_home_and_contact_pages_and_an_admin()
    {
        using var db = TestDb.Create();

        await DbSeeder.InitialiseAsync(db, "admin", "pw", new DateTime(2030, 1, 1));

        Assert.Equal(2, db.ContentPages.Count());
        Assert.Single(db.AdminUsers);
        Assert.NotEqual("pw", db.AdminUsers.Single().PasswordHash);   // stored as a hash, not the password
    }

    [Fact]
    public async Task Seeder_can_run_twice_without_duplicating_data()
    {
        using var db = TestDb.Create();

        await DbSeeder.InitialiseAsync(db, "admin", "pw", new DateTime(2030, 1, 1));
        await DbSeeder.InitialiseAsync(db, "admin", "pw", new DateTime(2030, 1, 1));

        Assert.Equal(2, db.ContentPages.Count());
        Assert.Single(db.AdminUsers);
    }

    [Fact]
    public async Task UpdateAsync_changes_the_saved_text()
    {
        using var db = TestDb.Create();
        await DbSeeder.InitialiseAsync(db, "admin", "pw", new DateTime(2030, 1, 1));
        var service = new ContentService(db);

        await service.UpdateAsync("home", new UpdateContentRequest("New title", "New body"));

        var reloaded = await service.GetAsync("home");
        Assert.Equal("New title", reloaded!.Title);
        Assert.Equal("New body", reloaded.Body);
    }

    [Fact]
    public async Task UpdateAsync_returns_null_for_an_unknown_page()
    {
        using var db = TestDb.Create();
        var service = new ContentService(db);

        Assert.Null(await service.UpdateAsync("nope", new UpdateContentRequest("t", "b")));
    }

    [Fact]
    public void PasswordHasher_accepts_the_right_password_and_rejects_the_wrong_one()
    {
        var hash = PasswordHasher.Hash("s3cret!");

        Assert.True(PasswordHasher.Verify("s3cret!", hash));
        Assert.False(PasswordHasher.Verify("wrong", hash));
    }

    [Fact]
    public void PasswordHasher_gives_a_different_hash_each_time_because_of_the_random_salt()
    {
        Assert.NotEqual(PasswordHasher.Hash("same"), PasswordHasher.Hash("same"));
    }

    [Fact]
    public async Task LoginAsync_returns_a_token_for_good_credentials_and_null_for_bad_ones()
    {
        using var db = TestDb.Create();
        await DbSeeder.InitialiseAsync(db, "admin", "pw", new DateTime(2030, 1, 1));
        var auth = new AuthService(db, Jwt);

        Assert.False(string.IsNullOrEmpty(await auth.LoginAsync("admin", "pw")));
        Assert.Null(await auth.LoginAsync("admin", "wrong"));
        Assert.Null(await auth.LoginAsync("nobody", "pw"));
    }
}
