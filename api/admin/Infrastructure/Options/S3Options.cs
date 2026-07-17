using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public sealed class S3Options
{
    private S3Options(
        string region,
        string bucket,
        string publicBaseUrl,
        string accessKeyId,
        string secretAccessKey)
    {
        Region = region;
        Bucket = bucket;
        PublicBaseUrl = publicBaseUrl.TrimEnd('/');
        AccessKeyId = accessKeyId;
        SecretAccessKey = secretAccessKey;
    }

    public string Region { get; }
    public string Bucket { get; }
    public string PublicBaseUrl { get; }
    public string AccessKeyId { get; }
    public string SecretAccessKey { get; }

    public static S3Options FromConfiguration(IConfiguration configuration)
    {
        return new S3Options(
            ReadRequired(configuration, "AWS_REGION"),
            ReadRequired(configuration, "AWS_S3_BUCKET"),
            ReadRequired(configuration, "AWS_S3_PUBLIC_BASE_URL"),
            ReadRequired(configuration, "AWS_ACCESS_KEY_ID"),
            ReadRequired(configuration, "AWS_SECRET_ACCESS_KEY"));
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
