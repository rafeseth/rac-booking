using RacBooking.Domain.Enums;

namespace RacBooking.Application.DTOs;

public record AppointmentResponse(
    Guid Id,
    Guid ClientId,
    Guid ProfessionalId,
    Guid ServiceId,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    string? Notes,
    decimal ServicePrice,
    decimal OriginalServicePrice,
    decimal FinalServicePrice,
    AttendanceLocationType AttendanceLocationType,
    string? ClientAddress,
    string? ClientNeighborhood,
    string? ClientAddressReference);
