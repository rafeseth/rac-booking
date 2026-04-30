using RacBooking.Domain.Enums;

namespace RacBooking.Application.DTOs;

public record CreateAppointmentRequest(
    Guid ClientId,
    Guid ProfessionalId,
    Guid ServiceId,
    DateTime StartTime,
    string? Notes,
    AttendanceLocationType AttendanceLocationType = AttendanceLocationType.Salon,
    string? ClientAddress = null,
    string? ClientNeighborhood = null,
    string? ClientAddressReference = null);
