using System.Text.RegularExpressions;

namespace Core.Entities;

public static partial class UserProfileRules
{
    public const int MinEmailLength = 5;
    public const int MaxEmailLength = 50;
    public const int MinFullNameLength = 2;
    public const int MaxFullNameLength = 50;
    public const int MaxPhoneNumberLength = 15;

    public const string FullNamePattern = @"^[\p{L}\p{M}][\p{L}\p{M} .'-]*[\p{L}\p{M}.]$";
    public const string PhoneNumberPattern = @"^(?:[0-9]{9,15}|\+[0-9]{9,14})$";

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public static string? NormalizeOptionalFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return null;
        }

        return RepeatedSpacesRegex().Replace(fullName.Trim(), " ");
    }

    public static string? NormalizeOptionalPhoneNumber(string? phoneNumber)
    {
        return string.IsNullOrWhiteSpace(phoneNumber)
            ? null
            : phoneNumber.Trim();
    }

    [GeneratedRegex(" {2,}", RegexOptions.CultureInvariant)]
    private static partial Regex RepeatedSpacesRegex();
}
