namespace Api.Constants;

using Core.Entities;

public static class ValidationConstants
{
    // Regex Patterns
    public const string PhonePattern = UserProfileRules.PhoneNumberPattern;
    public const string FullNamePattern = UserProfileRules.FullNamePattern;

    // Password Rules
    public const int MinPasswordLength = 5;
    public const int MaxPasswordLength = 200;

    // Username Rules
    public const int MinUsernameLength = 5;
    public const int MaxUsernameLength = 50;

    // FullName Rules
    public const int MinFullNameLength = UserProfileRules.MinFullNameLength;
    public const int MaxFullNameLength = UserProfileRules.MaxFullNameLength;

    // Email Rules
    public const int MinEmailLength = UserProfileRules.MinEmailLength;
    public const int MaxEmailLength = UserProfileRules.MaxEmailLength;
}
