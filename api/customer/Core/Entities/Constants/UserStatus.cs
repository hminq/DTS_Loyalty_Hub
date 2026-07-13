namespace Core.Entities.Constants;

public static class UserStatus
{
    public const string Enable = "ENABLE";

    public const string Disable = "DISABLE";

    public static bool IsEnabled(string status)
    {
        return string.Equals(status, Enable, StringComparison.OrdinalIgnoreCase);
    }
}
