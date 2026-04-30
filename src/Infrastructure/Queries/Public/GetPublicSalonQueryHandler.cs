using RacBooking.Application.DTOs;
using RacBooking.Application.Queries.Public.GetPublicSalon;
using RacBooking.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace RacBooking.Infrastructure.Queries.Public;

public sealed class GetPublicSalonQueryHandler : IRequestHandler<GetPublicSalonQuery, PublicSalonResponse?>
{
    private readonly AppDbContext _context;

    public GetPublicSalonQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PublicSalonResponse?> Handle(GetPublicSalonQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim();
        if (string.IsNullOrEmpty(slug))
            return null;

        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive, cancellationToken);

        if (tenant == null)
            return null;

        var tenantId = tenant.Id;

        var segments = await _context.ServiceSegments
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Select(s => new ServiceSegmentResponse(s.Id, s.Name, s.Slug, s.DisplayOrder))
            .ToListAsync(cancellationToken);

        var services = await _context.Services
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceResponse(
                s.Id,
                s.ServiceSegmentId,
                s.Name,
                s.Description,
                s.DurationMinutes,
                s.BufferAfterMinutes,
                s.Price))
            .ToListAsync(cancellationToken);

        var professionals = await _context.Professionals
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .OrderByDescending(p => p.IsPrimary)
            .ThenBy(p => p.Name)
            .Select(p => new ProfessionalResponse(p.Id, p.Name, p.IsPrimary))
            .ToListAsync(cancellationToken);

        var activeProfessionalIds = professionals.Select(p => p.Id).ToHashSet();
        var activeServiceIds = services.Select(s => s.Id).ToHashSet();

        var links = await _context.ProfessionalServices
            .AsNoTracking()
            .Where(ps =>
                ps.TenantId == tenantId &&
                activeProfessionalIds.Contains(ps.ProfessionalId) &&
                activeServiceIds.Contains(ps.ServiceId))
            .Select(ps => new PublicProfessionalServiceLink(ps.ProfessionalId, ps.ServiceId))
            .ToListAsync(cancellationToken);

        var branding = new PublicSalonBranding(
            tenant.PrimaryColor,
            tenant.SecondaryColor,
            tenant.AccentColor,
            tenant.BackgroundColor,
            tenant.LogoUrl,
            tenant.HeaderImageUrl,
            tenant.ClosingTime);

        return new PublicSalonResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            branding,
            string.IsNullOrWhiteSpace(tenant.TimeZoneId) ? null : tenant.TimeZoneId.Trim(),
            tenant.AttendanceMode,
            tenant.SalonAddressLine,
            tenant.SalonAddressReference,
            tenant.ServiceAreaDescription,
            segments,
            services,
            professionals,
            links);
    }
}
