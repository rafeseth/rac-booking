namespace RacBooking.Domain.Entities;

/// <summary>End customer of the tenant.</summary>
public class Client
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? BirthDate { get; set; }

    public string? AddressLine { get; set; }
    public string? Neighborhood { get; set; }
    public string? AddressReference { get; set; }
}
