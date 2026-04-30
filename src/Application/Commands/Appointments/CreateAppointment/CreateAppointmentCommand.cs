using RacBooking.Application.DTOs;
using MediatR;

namespace RacBooking.Application.Commands.Appointments.CreateAppointment;

/// <param name="TenantIdForBlockCheck">When set (public slug flow), enforces schedule blocks for this tenant even if context differs.</param>
public record CreateAppointmentCommand(CreateAppointmentRequest Request, Guid? TenantIdForBlockCheck = null)
    : IRequest<AppointmentResponse>;
