using Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace Scheduler.Tests.Options;

public sealed class RabbitMqConsumerOptionsTests
{
    [Fact]
    public void FromConfiguration_ValidValues_ReturnsOptions()
    {
        var options = RabbitMqConsumerOptions.FromConfiguration(
            Configuration(ValidValues()));

        Assert.Equal(new RabbitMqConsumerOptions(
            "broker.example",
            5672,
            "loyalty",
            "campaign-consumer",
            "secret",
            "dts.campaign.events",
            1,
            "dts-campaign-consumer"), options);
    }

    [Theory]
    [InlineData("RABBITMQ_HOST")]
    [InlineData("RABBITMQ_PORT")]
    [InlineData("RABBITMQ_VIRTUAL_HOST")]
    [InlineData("RABBITMQ_USERNAME")]
    [InlineData("RABBITMQ_PASSWORD")]
    [InlineData("RABBITMQ_QUEUE_NAME")]
    [InlineData("RABBITMQ_PREFETCH_COUNT")]
    [InlineData("RABBITMQ_CONNECTION_NAME")]
    public void FromConfiguration_MissingRequiredRabbitValue_Throws(string key)
    {
        var values = ValidValues();
        values.Remove(key);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            RabbitMqConsumerOptions.FromConfiguration(Configuration(values)));
        Assert.Contains(key, exception.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("65536")]
    [InlineData("invalid")]
    public void FromConfiguration_InvalidPort_Throws(string value)
    {
        var values = ValidValues();
        values["RABBITMQ_PORT"] = value;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            RabbitMqConsumerOptions.FromConfiguration(Configuration(values)));
        Assert.Contains("RABBITMQ_PORT", exception.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("65536")]
    [InlineData("invalid")]
    public void FromConfiguration_InvalidPrefetch_Throws(string value)
    {
        var values = ValidValues();
        values["RABBITMQ_PREFETCH_COUNT"] = value;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            RabbitMqConsumerOptions.FromConfiguration(Configuration(values)));
        Assert.Contains("RABBITMQ_PREFETCH_COUNT", exception.Message);
    }

    private static Dictionary<string, string?> ValidValues()
    {
        return new Dictionary<string, string?>
        {
            ["DATABASE_URL"] = "Host=localhost",
            ["RABBITMQ_HOST"] = "broker.example",
            ["RABBITMQ_PORT"] = "5672",
            ["RABBITMQ_VIRTUAL_HOST"] = "loyalty",
            ["RABBITMQ_USERNAME"] = "campaign-consumer",
            ["RABBITMQ_PASSWORD"] = "secret",
            ["RABBITMQ_QUEUE_NAME"] = "dts.campaign.events",
            ["RABBITMQ_PREFETCH_COUNT"] = "1",
            ["RABBITMQ_CONNECTION_NAME"] = "dts-campaign-consumer"
        };
    }

    private static IConfiguration Configuration(
        Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
