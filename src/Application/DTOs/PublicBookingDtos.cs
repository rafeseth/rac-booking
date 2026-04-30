using RacBooking.Domain.Enums;

namespace RacBooking.Application.DTOs;

/// <summary>Public salon bootstrap payload for the anonymous booking UI.</summary>
public record PublicSalonResponse(
    Guid TenantId,
    string Name,
    string Slug,
    PublicSalonBranding Branding,
    string? TimeZoneId,
    AttendanceMode AttendanceMode,
    string? SalonAddressLine,
    string? SalonAddressReference,
    string? ServiceAreaDescription,
    IReadOnlyList<ServiceSegmentResponse> Segments,
    IReadOnlyList<ServiceResponse> Services,
    IReadOnlyList<ProfessionalResponse> Professionals,
    IReadOnlyList<PublicProfessionalServiceLink> ProfessionalServices);

/// <summary>Theme and imagery safe for public pages (no storage keys).</summary>
public record PublicSalonBranding(
    string? PrimaryColor,
    string? SecondaryColor,
    string? AccentColor,
    string? BackgroundColor,
    string? LogoUrl,
    string? HeaderImageUrl,
    string? ClosingTime);

public record ServiceSegmentResponse(
    Guid Id,
    string Name,
    string Slug,
    int DisplayOrder);

public record ServiceResponse(
    Guid Id,
    Guid ServiceSegmentId,
    string Name,
    string Description,
    int DurationMinutes,
    int BufferAfterMinutes,
    decimal Price);

/// <summary>Professional shown on public booking (no contact details).</summary>
public record ProfessionalResponse(Guid Id, string Name, bool IsPrimary);

/// <summary>Maps which professionals can perform which services.</summary>
public record PublicProfessionalServiceLink(Guid ProfessionalId, Guid ServiceId);
