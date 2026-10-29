using SalonApi.Models;
using SalonApi.Services;

namespace SalonApi.Endpoints;

/// <summary>All HTTP routes. Public ones need no login; the "admin" ones need a valid token.</summary>
public static class ApiEndpoints
{
    public static void MapApi(this WebApplication app)
    {
        // ---- Public ----
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

        app.MapGet("/api/content/{key}", async (string key, ContentService content) =>
            await content.GetAsync(key.ToLowerInvariant()) is { } page
                ? Results.Ok(new ContentDto(page.Key, page.Title, page.Body))
                : Results.NotFound());

        app.MapGet("/api/bookings", async (BookingService bookings) =>
            Results.Ok(await bookings.PublicUpcomingAsync(DateTime.Now)));

        app.MapPost("/api/auth/login", async (LoginRequest request, AuthService auth) =>
            await auth.LoginAsync(request.Username, request.Password) is { } token
                ? Results.Ok(new LoginResponse(token, request.Username))
                : Results.Unauthorized());

        // ---- Admin (login required) ----
        var admin = app.MapGroup("/api/admin").RequireAuthorization();

        admin.MapGet("/bookings", async (BookingService bookings) =>
            Results.Ok((await bookings.AllAsync()).Select(ToAdminDto)));

        admin.MapPost("/bookings", async (NewBookingRequest request, BookingService bookings) =>
        {
            try
            {
                var created = await bookings.AddAsync(request);
                return Results.Created($"/api/admin/bookings/{created.Id}", ToAdminDto(created));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        admin.MapDelete("/bookings/{id:int}", async (int id, BookingService bookings) =>
            await bookings.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        admin.MapPut("/content/{key}", async (string key, UpdateContentRequest request, ContentService content) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest(new { error = "Title is required." });
            return await content.UpdateAsync(key.ToLowerInvariant(), request) is { } page
                ? Results.Ok(new ContentDto(page.Key, page.Title, page.Body))
                : Results.NotFound();
        });
    }

    private static AdminBookingDto ToAdminDto(Booking b) => new(b.Id, b.ClientName, b.Service, b.StartsAt, b.Notes);
}
