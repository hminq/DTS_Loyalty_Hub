using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public sealed class S3Options
{
    private const int DefaultPresignedUrlExpirationMinutes = 15;
    private const int MaximumPresignedUrlExpirationMinutes = 7 * 24 * 60;

    private S3Options(
        string region,
        string bucket,
        string accessKeyId,
        string secretAccessKey,
        int presignedUrlExpirationMinutes)
    {
        Region = region;
        Bucket = bucket;
        AccessKeyId = accessKeyId;
        SecretAccessKey = secretAccessKey;
        PresignedUrlExpirationMinutes = presignedUrlExpirationMinutes;
    }

    public string Region { get; }
    public string Bucket { get; }
    public string AccessKeyId { get; }
    public string SecretAccessKey { get; }
    public int PresignedUrlExpirationMinutes { get; }

    public static S3Options FromConfiguration(IConfiguration configuration)
    {
        return new S3Options(
            ReadRequired(configuration, "AWS_REGION"),
            ReadRequired(configuration, "AWS_S3_BUCKET"),
            ReadRequired(configuration, "AWS_ACCESS_KEY_ID"),
            ReadRequired(configuration, "AWS_SECRET_ACCESS_KEY"),
            ReadExpirationMinutes(configuration));
    }

    private static int ReadExpirationMinutes(IConfiguration configuration)
    {
        const string key = "AWS_S3_PRESIGNED_URL_EXPIRATION_MINUTES";
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            return DefaultPresignedUrlExpirationMinutes;
        }

        if (!int.TryParse(value, out var minutes) ||
            minutes <= 0 ||
            minutes > MaximumPresignedUrlExpirationMinutes)
        {
            throw new InvalidOperationException(
                $"{key} must be an integer between 1 and {MaximumPresignedUrlExpirationMinutes}.");
        }

        return minutes;
    }

    private static string ReadRequired(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required configuration value: {key}");
        }

        return value.Trim();
    }
}
