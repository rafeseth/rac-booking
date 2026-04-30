namespace RacBooking.Application.DTOs;

public record ClientResponse(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    DateOnly? BirthDate,
    string? AddressLine,
    string? Neighborhood,
    string? AddressReference);
