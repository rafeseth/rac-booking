namespace RacBooking.Application.DTOs;

/// <summary>Slot quality hint for UI (tight fit vs small trailing gap).</summary>
public enum SlotPriority
{
    PerfectFit = 0,
    Preferred = 1,
    Normal = 2,
    CreatesSmallGap = 3
}
