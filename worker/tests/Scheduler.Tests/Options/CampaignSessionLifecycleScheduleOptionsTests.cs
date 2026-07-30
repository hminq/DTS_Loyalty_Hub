using Microsoft.Extensions.Configuration;
using Scheduler.Options;
using Xunit;

namespace Scheduler.Tests.Options;

public sealed class CampaignSessionLifecycleScheduleOptionsTests
{
    [Fact]
    public void FromConfiguration_ValidValues_ReturnsOptions()
    {
        var configuration = Configuration(
            cron: "0 * * * * ?",
            batchSize: "100");

        var options = CampaignSessionLifecycleScheduleOptions.FromConfiguration(
            configuration);

        Assert.Equal("0 * * * * ?", options.Cron);
        Assert.Equal(100, options.BatchSize);
    }

    [Theory]
    [InlineData("0 * * * * ?", "0")]
    [InlineData("0 * * * * ?", "1001")]
    [InlineData("invalid", "100")]
    [InlineData("", "100")]
    [InlineData("0 * * * * ?", "")]
    public void FromConfiguration_InvalidValues_Throws(
        string cron,
        string batchSize)
    {
        var configuration = Configuration(cron, batchSize);

        Assert.Throws<InvalidOperationException>(() =>
            CampaignSessionLifecycleScheduleOptions.FromConfiguration(configuration));
    }

    private static IConfiguration Configuration(
        string cron,
        string batchSize)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CAMPAIGN_SESSION_LIFECYCLE_CRON"] = cron,
                ["CAMPAIGN_SESSION_LIFECYCLE_BATCH_SIZE"] = batchSize
            }!)
            .Build();
    }
}
