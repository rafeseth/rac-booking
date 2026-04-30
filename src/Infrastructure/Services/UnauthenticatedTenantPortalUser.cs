using RacBooking.Application.Interfaces;
using RacBooking.Domain.Enums;

namespace RacBooking.Infrastructure.Services;

/// <summary>Placeholder until tenant JWT is implemented; public availability runs as anonymous.</summary>
public sealed class UnauthenticatedTenantPortalUser : ICurrentTenantPortalUser
{
    public bool IsAuthenticated => false;
    public Guid UserId => Guid.Empty;
    public bool IsTenantAdmin => false;
    public bool IsProfessionalUser => false;
    public TenantPortalRole Role => TenantPortalRole.TenantAdmin;
    public Guid? LinkedProfessionalId => null;
}
