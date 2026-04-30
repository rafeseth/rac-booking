namespace RacBooking.Domain.Entities;

/// <summary>Recurring weekly working window for a professional.</summary>
public class ProfessionalWorkingHours
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProfessionalId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>Start of shift in salon-local time.</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>End of shift in salon-local time.</summary>
    public TimeSpan EndTime { get; set; }

    public Professional? Professional { get; set; }
}
