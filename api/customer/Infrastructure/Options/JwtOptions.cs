using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public sealed class JwtOptions
{
    private JwtOptions(
        string secret,
        string issuer,
        string audience,
        int expiresMinutes)
    {
        Secret = secret;
        Issuer = issuer;
        Audience = audience;
        ExpiresMinutes = expiresMinutes;
    }

    public string Secret { get; }

    public string Issuer { get; }

    public string Audience { get; }

    public int ExpiresMinutes { get; }

    public static JwtOptions FromConfiguration(IConfiguration configuration)
    {
        var secret = ReadRequired(configuration, "JWT_SECRET");
        var issuer = ReadRequired(configuration, "JWT_ISSUER");
        var audience = ReadRequired(configuration, "JWT_AUDIENCE");
        var expiresMinutesValue = ReadRequired(configuration, "JWT_EXPIRES_MINUTES");

        if (secret.Length < 32)
        {
            throw new InvalidOperationException("JWT_SECRET must be at least 32 characters.");
        }

        int expiresMinutes = 0;

        if (!string.IsNullOrWhiteSpace(expiresMinutesValue) &&
            (!int.TryParse(expiresMinutesValue, out expiresMinutes) || expiresMinutes <= 0))
        {
            throw new InvalidOperationException("JWT_EXPIRES_MINUTES must be a positive integer.");
        }

        return new JwtOptions(
            secret,
            issuer,
            audience,
            expiresMinutes);
    }

    private static string ReadRequired(IConfiguration configuration, string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required configuration value: {key}");
        }

        return value;
    }
}
