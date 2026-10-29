using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SalonApi.Data;

namespace SalonApi.Services;

public class JwtSettings
{
    public string Key { get; set; } = "";          // secret used to sign tokens (at least 32 characters)
    public string Issuer { get; set; } = "salon-api";
    public int ExpiryHours { get; set; } = 8;
}

/// <summary>Checks a username and password and, if right, hands back a signed token (JWT).</summary>
public class AuthService(SalonDbContext db, JwtSettings jwt)
{
    /// <summary>Returns a token, or null when the login is wrong. Same answer for "no such user" and "bad password".</summary>
    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await db.AdminUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash)) return null;

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            claims: [new Claim(ClaimTypes.Name, user.Username)],
            expires: DateTime.UtcNow.AddHours(jwt.ExpiryHours),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
