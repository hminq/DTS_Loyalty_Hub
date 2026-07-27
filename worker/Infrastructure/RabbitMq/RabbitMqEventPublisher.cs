using System.Text;
using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Infrastructure.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Infrastructure.RabbitMq;

public sealed class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqPublisherOptions _options;
    private readonly SemaphoreSlim _channelLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqEventPublisher(RabbitMqPublisherOptions options)
    {
        _options = options;
    }

    public async Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken)
    {
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
                _options.ExchangeName,
                message.RoutingKey,
                mandatory: true,
                properties,
                Encoding.UTF8.GetBytes(message.Body),
                cancellationToken);
        }
        catch (PublishException exception)
        {
            throw new OutboxPublishException(
                exception.IsReturn
                    ? OutboxDispatchErrorCodes.PublishNotRouted
                    : OutboxDispatchErrorCodes.PublishNotConfirmed,
                exception.Message,
                exception);
        }
        catch (BrokerUnreachableException exception)
        {
            throw new OutboxPublishException(
                OutboxDispatchErrorCodes.BrokerUnreachable,
                exception.Message,
                exception);
        }
        catch (RabbitMQClientException exception)
        {
            throw new OutboxPublishException(
                OutboxDispatchErrorCodes.BrokerUnreachable,
                exception.Message,
                exception);
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
        if (_connection is null || !_connection.IsOpen)
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.Username,
                Password = _options.Password,
                ClientProvidedName = _options.ConnectionName,
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
