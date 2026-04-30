namespace RacBooking.Domain.Entities;

/// <summary>Many-to-many: which services a professional can perform.</summary>
public class ProfessionalService
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProfessionalId { get; set; }
    public Guid ServiceId { get; set; }

    public Professional? Professional { get; set; }
    public Service? Service { get; set; }
}
