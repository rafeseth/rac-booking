using RacBooking.Domain.Enums;

namespace RacBooking.Application.DTOs;

/// <summary>Minimal shape for FullCalendar-style lists (admin calendar).</summary>
public record AppointmentCalendarItemResponse(
    Guid Id,
    string Title,
    DateTime Start,
    DateTime End,
    AppointmentStatus Status,
    string ServiceName,
    string ProfessionalName,
    string ClientName,
    decimal ServicePrice);
