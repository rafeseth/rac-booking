namespace RacBooking.Domain.Enums;

/// <summary>Tenant portal credential role (admin UI). Phase 2 uses an unauthenticated stub until JWT is added.</summary>
public enum TenantPortalRole
{
    TenantAdmin = 0,
    ProfessionalUser = 1,
    Receptionist = 2
}
