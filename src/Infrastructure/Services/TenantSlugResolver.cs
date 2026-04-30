using RacBooking.Application.Common.Interfaces;
using RacBooking.Domain.Entities;
using RacBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RacBooking.Infrastructure.Services;

/// <summary>Loads <see cref="Tenant"/> by public slug (not scoped by global tenant filters).</summary>
public class TenantSlugResolver : ITenantSlugResolver
{
    private readonly AppDbContext _context;

    public TenantSlugResolver(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive, cancellationToken);
    }
}
