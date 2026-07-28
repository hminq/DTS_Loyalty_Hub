using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public sealed record RabbitMqConsumerOptions(
    string Host,
    int Port,
    string VirtualHost,
    string Username,
    string Password,
    string QueueName,
    ushort PrefetchCount,
    string ConnectionName)
{
    public static RabbitMqConsumerOptions FromConfiguration(
        IConfiguration configuration)
    {
        return new RabbitMqConsumerOptions(
            Required(configuration, "RABBITMQ_HOST"),
            RequiredPort(configuration),
            Required(configuration, "RABBITMQ_VIRTUAL_HOST"),
            Required(configuration, "RABBITMQ_USERNAME"),
            Required(configuration, "RABBITMQ_PASSWORD"),
            Required(configuration, "RABBITMQ_QUEUE_NAME"),
            RequiredPrefetchCount(configuration),
            Required(configuration, "RABBITMQ_CONNECTION_NAME"));
    }

    private static string Required(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing required configuration value: {key}");
        }

        return value;
    }

    private static int RequiredPort(IConfiguration configuration)
    {
        var rawValue = Required(configuration, "RABBITMQ_PORT");
        if (!int.TryParse(rawValue, out var port) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "RABBITMQ_PORT must be an integer between 1 and 65535.");
        }

        return port;
    }

    private static ushort RequiredPrefetchCount(IConfiguration configuration)
    {
        var rawValue = Required(configuration, "RABBITMQ_PREFETCH_COUNT");
        if (!ushort.TryParse(rawValue, out var prefetchCount) ||
            prefetchCount == 0)
        {
            throw new InvalidOperationException(
                "RABBITMQ_PREFETCH_COUNT must be an integer between 1 and 65535.");
        }

        return prefetchCount;
    }
}
