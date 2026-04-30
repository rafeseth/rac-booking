using RacBooking.Application.Common.Interfaces;
using RacBooking.Application.Interfaces;
using RacBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RacBooking.Infrastructure.Services;

public sealed class CurrentTenantTimeZoneProvider : ICurrentTenantTimeZoneProvider
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public CurrentTenantTimeZoneProvider(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<string> GetTimeZoneIdAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant context is required.");

        var tz = await _db.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenantId)
            .Select(t => t.TimeZoneId)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(tz) ? "America/Sao_Paulo" : tz.Trim();
    }
}
