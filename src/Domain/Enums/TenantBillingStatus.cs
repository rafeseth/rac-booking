namespace RacBooking.Domain.Enums;

/// <summary>Commercial status of the tenant on the SaaS platform.</summary>
public enum TenantBillingStatus
{
    Active = 0,
    Courtesy = 1,
    Suspended = 2,
    Canceled = 3
}
