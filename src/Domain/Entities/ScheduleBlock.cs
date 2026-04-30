namespace RacBooking.Domain.Entities;

/// <summary>Time range when a professional is unavailable (break, PTO, etc.).</summary>
public class ScheduleBlock
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProfessionalId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }

    public string Type { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }

    public Professional? Professional { get; set; }
}
