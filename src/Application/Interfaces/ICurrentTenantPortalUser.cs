using RacBooking.Domain.Enums;

namespace RacBooking.Application.Interfaces;

/// <summary>Current tenant portal user from JWT. Anonymous for public availability until auth is wired.</summary>
public interface ICurrentTenantPortalUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    bool IsTenantAdmin { get; }
    bool IsProfessionalUser { get; }
    TenantPortalRole Role { get; }
    Guid? LinkedProfessionalId { get; }
    bool IsProfessionalRestricted => IsProfessionalUser && LinkedProfessionalId.HasValue;
}
