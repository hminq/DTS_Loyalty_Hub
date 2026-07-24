using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public sealed record VoucherPoolImportOptions(
    string Region,
    string Bucket,
    string AccessKeyId,
    string SecretAccessKey,
    long MaximumFileSizeBytes)
{
    public static VoucherPoolImportOptions FromConfiguration(IConfiguration configuration)
    {
        return new VoucherPoolImportOptions(
            ReadRequired(configuration, "AWS_REGION"),
            ReadRequired(configuration, "AWS_S3_BUCKET"),
            ReadRequired(configuration, "AWS_ACCESS_KEY_ID"),
            ReadRequired(configuration, "AWS_SECRET_ACCESS_KEY"),
            ReadPositiveLong(configuration, "VOUCHER_POOL_IMPORT_MAX_FILE_SIZE_BYTES"));
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

    private static long ReadPositiveLong(IConfiguration configuration, string key)
    {
        if (!long.TryParse(configuration[key], out var value) || value <= 0)
        {
            throw new InvalidOperationException($"{key} must be a positive integer.");
        }

        return value;
    }
}
