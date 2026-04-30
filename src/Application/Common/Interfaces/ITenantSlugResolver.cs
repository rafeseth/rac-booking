using RacBooking.Domain.Entities;

namespace RacBooking.Application.Common.Interfaces;

/// <summary>Resolves a tenant by public URL slug.</summary>
public interface ITenantSlugResolver
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
