using Microsoft.Extensions.Configuration;
using Scheduler.Options;

namespace Scheduler.Tests.Options;

public sealed class VoucherPoolGenerationScheduleOptionsTests
{
    [Fact]
    public void FromConfiguration_ValidValues_ReturnsOptions()
    {
        var configuration = Configuration(
            cron: "0/10 * * * * ?",
            timeZone: "Asia/Ho_Chi_Minh",
            batchSize: "5000");

        var options = VoucherPoolGenerationScheduleOptions.FromConfiguration(
            configuration);

        Assert.Equal("0/10 * * * * ?", options.Cron);
        Assert.Equal("Asia/Ho_Chi_Minh", options.TimeZone);
        Assert.Equal(5000, options.BatchSize);
    }

    [Theory]
    [InlineData("0/10 * * * * ?", "Asia/Ho_Chi_Minh", "0")]
    [InlineData("0/10 * * * * ?", "Asia/Ho_Chi_Minh", "10001")]
    [InlineData("invalid", "Asia/Ho_Chi_Minh", "5000")]
    [InlineData("0/10 * * * * ?", "invalid/time-zone", "5000")]
    public void FromConfiguration_InvalidValues_Throws(
        string cron,
        string timeZone,
        string batchSize)
    {
        var configuration = Configuration(cron, timeZone, batchSize);

        Assert.Throws<InvalidOperationException>(() =>
            VoucherPoolGenerationScheduleOptions.FromConfiguration(configuration));
    }

    private static IConfiguration Configuration(
        string cron,
        string timeZone,
        string batchSize)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VOUCHER_POOL_GENERATION_CRON"] = cron,
                ["VOUCHER_POOL_GENERATION_TIME_ZONE"] = timeZone,
                ["VOUCHER_POOL_GENERATION_BATCH_SIZE"] = batchSize
            })
            .Build();
    }
}
