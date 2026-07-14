namespace Api.Constants;

public static class ValidationConstants
{
    // Regex Patterns
    public const string PhonePattern = @"^\+?[0-9]{9,15}$";

    // Password Rules
    public const int MinPasswordLength = 5;
    public const int MaxPasswordLength = 200;

    // Username Rules
    public const int MinUsernameLength = 5;
    public const int MaxUsernameLength = 50;

    // FullName Rules
    public const int MinFullNameLength = 5;
    public const int MaxFullNameLength = 50;

    // Email Rules
    public const int MinEmailLength = 5;
    public const int MaxEmailLength = 50;
}