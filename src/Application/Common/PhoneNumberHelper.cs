using System.Text.RegularExpressions;

namespace RacBooking.Application.Common;

/// <summary>Minimal international-style phone normalization (E.164-like: + and 8–15 digits). Suitable for portfolio demos.</summary>
public static class PhoneNumberHelper
{
    private static readonly Regex NonDigitRegex = new(@"\D", RegexOptions.Compiled);

    /// <summary>Extract digits only (ignores leading + and separators).</summary>
    public static string GetDigitsOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return NonDigitRegex.Replace(value, "");
    }

    /// <summary>Returns a normalized E.164-style value (+country and national digits) or null if invalid.</summary>
    public static string? Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        var digits = GetDigitsOnly(phone);
        // ITU-T E.164: maximum 15 digits; minimum sensible length for international demos
        if (digits.Length is < 8 or > 15) return null;
        if (digits[0] == '0') return null;
        if (digits.All(static c => c == '0')) return null;
        return "+" + digits;
    }
}
