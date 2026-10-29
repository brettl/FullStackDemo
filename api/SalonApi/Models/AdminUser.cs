namespace SalonApi.Models;

/// <summary>A person who can log in to the admin area. We store a password HASH, never the password.</summary>
public class AdminUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}
