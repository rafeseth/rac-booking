using RacBooking.Domain.Enums;

namespace RacBooking.Domain.Entities;

/// <summary>Scheduled service instance for a client with a professional.</summary>
public class Appointment
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public Guid ClientId { get; set; }
    public Guid ProfessionalId { get; set; }
    public Guid ServiceId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }

    public decimal OriginalServicePrice { get; set; }
    public decimal FinalServicePrice { get; set; }
    public decimal ServicePrice { get; set; }

    public AttendanceLocationType AttendanceLocationType { get; set; } = AttendanceLocationType.Salon;

    public string? ClientAddress { get; set; }
    public string? ClientNeighborhood { get; set; }
    public string? ClientAddressReference { get; set; }

    public Client? Client { get; set; }
    public Professional? Professional { get; set; }
    public Service? Service { get; set; }
}
