namespace RacBooking.Application.Exceptions;

/// <summary>Business validation failure (e.g. domicile address). Maps to HTTP 400.</summary>
public class AppointmentValidationException : Exception
{
    public AppointmentValidationException(string message)
        : base(message)
    {
    }
}
