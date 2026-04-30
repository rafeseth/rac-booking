using RacBooking.Domain.Enums;

namespace RacBooking.Application.DTOs;

/// <summary>Anonymous public booking request (no client login).</summary>
public record PublicCreateAppointmentRequest(
    string Name,
    string Phone,
    string? Email,
    Guid ProfessionalId,
    Guid ServiceId,
    DateTime StartTime,
    string? Notes,
    AttendanceLocationType? AttendanceLocationType = null,
    string? ClientAddress = null,
    string? ClientNeighborhood = null,
    string? ClientAddressReference = null);
