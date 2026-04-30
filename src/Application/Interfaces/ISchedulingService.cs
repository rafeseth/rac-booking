using RacBooking.Application.DTOs;

namespace RacBooking.Application.Interfaces;

/// <summary>Scheduling, availability, and appointment creation.</summary>
public interface ISchedulingService
{
    Task<IReadOnlyList<AvailableSlotResponse>> GetAvailableSlotsAsync(
        Guid professionalId,
        DateTime date,
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<AppointmentResponse> CreateAppointmentAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default,
        Guid? tenantIdForBlockCheck = null);
}
