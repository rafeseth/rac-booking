namespace RacBooking.Domain.Enums;

/// <summary>Lifecycle state of a booked appointment.</summary>
public enum AppointmentStatus
{
    Scheduled = 0,
    Confirmed = 1,
    Completed = 2,
    Canceled = 3,
    NoShow = 4
}
