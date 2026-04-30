using RacBooking.Application.DTOs;
using RacBooking.Application.Interfaces;
using MediatR;

namespace RacBooking.Application.Queries.Availability.GetAvailableSlots;

public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>>
{
    private readonly ISchedulingService _schedulingService;

    public GetAvailableSlotsQueryHandler(ISchedulingService schedulingService)
    {
        _schedulingService = schedulingService;
    }

    public Task<IReadOnlyList<AvailableSlotResponse>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        return _schedulingService.GetAvailableSlotsAsync(
            request.ProfessionalId,
            request.Date,
            request.ServiceId,
            cancellationToken);
    }
}
