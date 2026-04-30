namespace RacBooking.Domain.Entities;

/// <summary>Staff member who performs services for the tenant.</summary>
public class Professional
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}
