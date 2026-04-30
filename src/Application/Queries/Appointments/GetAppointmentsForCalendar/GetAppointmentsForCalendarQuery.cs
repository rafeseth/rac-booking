using RacBooking.Application.DTOs;
using MediatR;

namespace RacBooking.Application.Queries.Appointments.GetAppointmentsForCalendar;

public record GetAppointmentsForCalendarQuery(
    DateOnly From,
    DateOnly To,
    Guid? ProfessionalId = null) : IRequest<IReadOnlyList<AppointmentCalendarItemResponse>>;
