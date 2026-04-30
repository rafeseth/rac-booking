using RacBooking.Application.DTOs;
using RacBooking.Application.Interfaces;
using MediatR;

namespace RacBooking.Application.Commands.Appointments.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentResponse>
{
    private readonly ISchedulingService _schedulingService;

    public CreateAppointmentCommandHandler(ISchedulingService schedulingService)
    {
        _schedulingService = schedulingService;
    }

    public Task<AppointmentResponse> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        return _schedulingService.CreateAppointmentAsync(
            request.Request,
            cancellationToken,
            request.TenantIdForBlockCheck);
    }
}
