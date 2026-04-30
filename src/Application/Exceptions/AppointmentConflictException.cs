using RacBooking.Application.DTOs;

namespace RacBooking.Application.Exceptions;

/// <summary>Booking conflicts with another appointment or a schedule block; may include same-day alternatives.</summary>
public class AppointmentConflictException : InvalidOperationException
{
    public IReadOnlyList<AvailableSlotResponse> AlternativeSlots { get; }

    public AppointmentConflictException(string message, IReadOnlyList<AvailableSlotResponse>? alternativeSlots = null)
        : base(message)
    {
        AlternativeSlots = alternativeSlots ?? Array.Empty<AvailableSlotResponse>();
    }
}
