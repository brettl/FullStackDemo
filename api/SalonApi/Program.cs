using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SalonApi.Data;
using SalonApi.Endpoints;
using SalonApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Settings come from environment variables (see docker-compose.yml) ----
var connectionString = builder.Configuration.GetConnectionString("Salon")
    ?? throw new InvalidOperationException("ConnectionStrings__Salon is not set.");
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (jwt.Key.Length < 32) throw new InvalidOperationException("Jwt__Key must be at least 32 characters.");

// ---- Services (dependency injection) ----
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));
builder.Services.AddSingleton(jwt);
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---- Create the database and starter data. MySQL can take a few seconds to start, so retry. ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
    var adminUser = builder.Configuration["Admin:Username"] ?? "admin";
    var adminPassword = builder.Configuration["Admin:Password"] ?? throw new InvalidOperationException("Admin__Password is not set.");

    for (var attempt = 1; ; attempt++)
    {
        try
        {
            await DbSeeder.InitialiseAsync(db, adminUser, adminPassword, DateTime.Today);
            break;
        }
        catch (Exception ex) when (attempt < 30)
        {
            app.Logger.LogWarning("Database not ready yet (attempt {Attempt}): {Message}", attempt, ex.Message);
            await Task.Delay(2000);
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapApi();
app.Run();

// Lets the test project reference this class if it ever wants to start the whole API.
public partial class Program;
