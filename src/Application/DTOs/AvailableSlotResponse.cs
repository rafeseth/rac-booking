namespace RacBooking.Application.DTOs;

/// <summary>One bookable slot: UTC for persistence; display fields in tenant time zone.</summary>
public record AvailableSlotResponse(
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string DisplayTime,
    string DisplayDate,
    SlotPriority Priority = SlotPriority.Normal);
