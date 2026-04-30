namespace RacBooking.Application.Interfaces;

/// <summary>Provides the current tenant id for the request (JWT, header, or public slug override).</summary>
public interface ITenantProvider
{
    Guid TenantId { get; }
}
