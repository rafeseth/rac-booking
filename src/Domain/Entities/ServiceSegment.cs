namespace RacBooking.Domain.Entities;

/// <summary>Category/segment for services (per tenant).</summary>
public class ServiceSegment
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Stable slug, unique per tenant.</summary>
    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
