namespace RacBooking.Domain.Entities;

/// <summary>Bookable service in the tenant catalog.</summary>
public class Service
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    /// <summary>Buffer after the service; total blocked window = duration + buffer.</summary>
    public int BufferAfterMinutes { get; set; }

    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid ServiceSegmentId { get; set; }
    public ServiceSegment? ServiceSegment { get; set; }
}
