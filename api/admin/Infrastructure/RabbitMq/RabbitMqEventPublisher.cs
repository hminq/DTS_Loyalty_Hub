using System.Text;
using Core.Abstractions;
using Core.Entities.Events;
using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Infrastructure.RabbitMq;

public sealed class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly Lazy<RabbitMqPublisherOptions> _options;
    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqEventPublisher(IConfiguration configuration)
    {
        _options = new Lazy<RabbitMqPublisherOptions>(
            () => RabbitMqPublisherOptions.FromConfiguration(configuration));
    }

    public async Task PublishAsync(OutgoingEvent message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        await _channelLock.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetChannelAsync(cancellationToken);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true,
                MessageId = message.EventId.ToString(),
                Type = message.EventType
            };

            await channel.BasicPublishAsync(
                _options.Value.ExchangeName,
                message.RoutingKey,
                mandatory: true,
                properties,
                Encoding.UTF8.GetBytes(message.Body),
                cancellationToken);
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _channelLock.WaitAsync();
        try
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }
        }
        finally
        {
            _channelLock.Release();
            _channelLock.Dispose();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        var options = _options.Value;

        if (_connection is null || !_connection.IsOpen)
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }

            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                VirtualHost = options.VirtualHost,
                UserName = options.Username,
                Password = options.Password,
                ClientProvidedName = options.ConnectionName,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = false
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = null;
        }

        if (_channel is null || !_channel.IsOpen)
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            _channel = await _connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);
        }

        return _channel;
    }
}
