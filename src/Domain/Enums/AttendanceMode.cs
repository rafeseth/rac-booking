namespace RacBooking.Domain.Enums;

/// <summary>How the tenant offers service: on-premise, at client address, or both.</summary>
public enum AttendanceMode
{
    Salon = 0,
    ClientAddress = 1,
    Both = 2
}
