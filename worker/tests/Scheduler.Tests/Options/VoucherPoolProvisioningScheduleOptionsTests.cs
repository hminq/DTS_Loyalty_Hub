using Microsoft.Extensions.Configuration;
using Scheduler.Options;

namespace Scheduler.Tests.Options;

public sealed class VoucherPoolProvisioningScheduleOptionsTests
{
    [Fact]
    public void FromConfiguration_ValidValues_ReturnsOptions()
    {
        var configuration = Configuration(
            cron: "0/10 * * * * ?",
            timeZone: "Asia/Ho_Chi_Minh",
            batchSize: "5000");

        var options = VoucherPoolProvisioningScheduleOptions.FromConfiguration(
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
            VoucherPoolProvisioningScheduleOptions.FromConfiguration(configuration));
    }

    private static IConfiguration Configuration(
        string cron,
        string timeZone,
        string batchSize)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VOUCHER_POOL_PROVISIONING_CRON"] = cron,
                ["VOUCHER_POOL_PROVISIONING_TIME_ZONE"] = timeZone,
                ["VOUCHER_POOL_PROVISIONING_BATCH_SIZE"] = batchSize
            })
            .Build();
    }
}
