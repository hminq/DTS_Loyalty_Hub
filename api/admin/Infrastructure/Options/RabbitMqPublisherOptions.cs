using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public sealed record RabbitMqPublisherOptions(
    string Host,
    int Port,
    string VirtualHost,
    string Username,
    string Password,
    string ExchangeName,
    string ConnectionName)
{
    public static RabbitMqPublisherOptions FromConfiguration(IConfiguration configuration)
    {
        return new RabbitMqPublisherOptions(
            Required(configuration, "RABBITMQ_HOST"),
            RequiredPort(configuration),
            Required(configuration, "RABBITMQ_VIRTUAL_HOST"),
            Required(configuration, "RABBITMQ_USERNAME"),
            Required(configuration, "RABBITMQ_PASSWORD"),
            Required(configuration, "RABBITMQ_EXCHANGE_NAME"),
            Required(configuration, "RABBITMQ_CONNECTION_NAME"));
    }

    private static string Required(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required configuration value: {key}");
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
}
