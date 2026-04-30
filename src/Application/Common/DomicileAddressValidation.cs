using System.Linq;

namespace RacBooking.Application.Common;

/// <summary>Minimal validation for client-address attendance (no postal geocoding).</summary>
public static class DomicileAddressValidation
{
    public const string InvalidMessage = "Provide a valid address for service at the client's location.";

    public static bool TryNormalizeAddressLine(string? raw, out string? normalized, out string? errorMessage)
    {
        normalized = null;
        errorMessage = null;
        var t = raw?.Trim();
        if (string.IsNullOrEmpty(t))
        {
            errorMessage = InvalidMessage;
            return false;
        }

        if (t.Length > 300)
        {
            errorMessage = "Address line is too long.";
            return false;
        }

        if (t.Length < 10 || !t.Any(char.IsDigit) || !t.Any(char.IsLetter))
        {
            errorMessage = InvalidMessage;
            return false;
        }

        normalized = t;
        return true;
    }

    public static bool TryNormalizeNeighborhood(string? raw, out string? normalized, out string? errorMessage)
    {
        normalized = null;
        errorMessage = null;
        var t = raw?.Trim();
        if (string.IsNullOrEmpty(t))
        {
            errorMessage = InvalidMessage;
            return false;
        }

        if (t.Length < 2 || t.Length > 120)
        {
            errorMessage = InvalidMessage;
            return false;
        }

        normalized = t;
        return true;
    }

    public static bool TryNormalizeReference(string? raw, out string? normalized, out string? errorMessage)
    {
        normalized = null;
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(raw))
            return true;
        var t = raw.Trim();
        if (t.Length > 200)
        {
            errorMessage = "Address reference is too long.";
            return false;
        }

        normalized = t;
        return true;
    }
}
