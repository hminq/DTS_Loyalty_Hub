using Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace Scheduler.Tests.Options;

public sealed class VoucherPoolImportOptionsTests
{
    [Fact]
    public void FromConfiguration_ValidValues_ReturnsOptions()
    {
        var configuration = Configuration();

        var options = VoucherPoolImportOptions.FromConfiguration(configuration);

        Assert.Equal("ap-southeast-1", options.Region);
        Assert.Equal("private-bucket", options.Bucket);
        Assert.Equal("access-key", options.AccessKeyId);
        Assert.Equal("secret-key", options.SecretAccessKey);
        Assert.Equal(268_435_456, options.MaximumFileSizeBytes);
    }

    [Theory]
    [InlineData("AWS_REGION")]
    [InlineData("AWS_S3_BUCKET")]
    [InlineData("AWS_ACCESS_KEY_ID")]
    [InlineData("AWS_SECRET_ACCESS_KEY")]
    public void FromConfiguration_MissingRequiredValue_Throws(string missingKey)
    {
        var values = ValidValues();
        values[missingKey] = null;
        var configuration = Configuration(values);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            VoucherPoolImportOptions.FromConfiguration(configuration));

        Assert.Equal(
            $"Missing required configuration value: {missingKey}",
            exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("not-a-number")]
    public void FromConfiguration_InvalidMaximumFileSize_Throws(string? value)
    {
        var values = ValidValues();
        values["VOUCHER_POOL_IMPORT_MAX_FILE_SIZE_BYTES"] = value;
        var configuration = Configuration(values);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            VoucherPoolImportOptions.FromConfiguration(configuration));

        Assert.Equal(
            "VOUCHER_POOL_IMPORT_MAX_FILE_SIZE_BYTES must be a positive integer.",
            exception.Message);
    }

    private static IConfiguration Configuration(
        Dictionary<string, string?>? values = null)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? ValidValues())
            .Build();
    }

    private static Dictionary<string, string?> ValidValues()
    {
        return new Dictionary<string, string?>
        {
            ["AWS_REGION"] = "ap-southeast-1",
            ["AWS_S3_BUCKET"] = "private-bucket",
            ["AWS_ACCESS_KEY_ID"] = "access-key",
            ["AWS_SECRET_ACCESS_KEY"] = "secret-key",
            ["VOUCHER_POOL_IMPORT_MAX_FILE_SIZE_BYTES"] = "268435456"
        };
    }
}
