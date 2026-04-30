using RacBooking.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace RacBooking.Api.Tenancy;

/// <summary>
/// Resolves tenant id: <c>TenantIdOverride</c> in <see cref="HttpContext.Items"/> (public booking),
/// then JWT <c>tenant_id</c> / <c>tenant</c>, then <c>X-Tenant-Id</c> header. Authentication is added in a later phase.
/// </summary>
public class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Guid.Empty;

            if (httpContext.Items.TryGetValue("TenantIdOverride", out var overrideValue))
            {
                if (overrideValue is Guid guidOverride)
                    return guidOverride;
                if (overrideValue is string s && Guid.TryParse(s, out var parsedOverride))
                    return parsedOverride;
            }

            var tenantClaim = httpContext.User?.FindFirst("tenant_id")
                ?? httpContext.User?.FindFirst("tenant");
            if (tenantClaim is { Value: not null } && Guid.TryParse(tenantClaim.Value, out var claimTenant))
                return claimTenant;

            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var values))
            {
                var raw = values.FirstOrDefault();
                if (Guid.TryParse(raw, out var headerTenant))
                    return headerTenant;
            }

            return Guid.Empty;
        }
    }
}
