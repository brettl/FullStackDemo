namespace SalonApi.Models;

// DTOs ("data transfer objects") are the shapes we send over HTTP. They let us hide fields we do not want public.

/// <summary>What the public Booking page may see: first name, service and time only.</summary>
public record PublicBookingDto(string FirstName, string Service, DateTime StartsAt);

/// <summary>What the admin sees: everything.</summary>
public record AdminBookingDto(int Id, string ClientName, string Service, DateTime StartsAt, string? Notes);

public record NewBookingRequest(string ClientName, string Service, DateTime StartsAt, string? Notes);

public record ContentDto(string Key, string Title, string Body);

public record UpdateContentRequest(string Title, string Body);

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string Username);
