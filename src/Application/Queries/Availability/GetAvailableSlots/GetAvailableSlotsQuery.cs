using RacBooking.Application.DTOs;
using MediatR;

namespace RacBooking.Application.Queries.Availability.GetAvailableSlots;

public record GetAvailableSlotsQuery(Guid ProfessionalId, DateTime Date, Guid ServiceId)
    : IRequest<IReadOnlyList<AvailableSlotResponse>>;
