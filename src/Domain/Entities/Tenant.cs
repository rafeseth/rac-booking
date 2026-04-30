using RacBooking.Domain.Enums;

namespace RacBooking.Domain.Entities;

/// <summary>Tenant (business workspace) in the multi-tenant scheduling platform.</summary>
public class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    /// <summary>Optional contact number for public UX (not integrated messaging in this phase).</summary>
    public string WhatsappNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? LogoObjectKey { get; set; }

    /// <summary>IANA time zone (e.g. America/Sao_Paulo) for salon-local display rules.</summary>
    public string? TimeZoneId { get; set; }

    /// <summary>Local closing time of business day, HH:mm (optional).</summary>
    public string? ClosingTime { get; set; }

    public string? HeaderImageUrl { get; set; }
    public string? HeaderImageObjectKey { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal MonthlyFee { get; set; }
    public DateTime JoinedAt { get; set; }

    public TenantBillingStatus BillingStatus { get; set; } = TenantBillingStatus.Active;
    public DateTime? CourtesyUntil { get; set; }

    public AttendanceMode AttendanceMode { get; set; } = AttendanceMode.Salon;

    public string? SalonAddressLine { get; set; }
    public string? SalonAddressReference { get; set; }
    public string? ServiceAreaDescription { get; set; }
}
