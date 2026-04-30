namespace RacBooking.Application.Common.Interfaces;

/// <summary>Resolves the IANA time zone for the current tenant (display rules for slots).</summary>
public interface ICurrentTenantTimeZoneProvider
{
    Task<string> GetTimeZoneIdAsync(CancellationToken cancellationToken = default);
}
