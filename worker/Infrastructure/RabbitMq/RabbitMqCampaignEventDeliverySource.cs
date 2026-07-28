using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.RabbitMq;

public sealed class RabbitMqCampaignEventDeliverySource
    : ICampaignEventDeliverySource
{
    private readonly RabbitMqConsumerOptions _options;
    private readonly ILogger<RabbitMqCampaignEventDeliverySource> _logger;

    public RabbitMqCampaignEventDeliverySource(
        RabbitMqConsumerOptions options,
        ILogger<RabbitMqCampaignEventDeliverySource> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync(
        Func<
            CampaignEventDelivery,
            CancellationToken,
            Task<CampaignEventDeliveryResult>> deliveryHandler,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(deliveryHandler);

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.Username,
            Password = _options.Password,
            ClientProvidedName = _options.ConnectionName,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = false,
            ConsumerDispatchConcurrency = 1
        };

        await using var connection =
            await factory.CreateConnectionAsync(cancellationToken);
        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.PrefetchCount,
            global: false,
            cancellationToken);

        var fatalFailure = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var subscriptionLock = new SemaphoreSlim(1, 1);
        var consumer = new AsyncEventingBasicConsumer(channel);
        string? consumerTag = null;

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var delivery = new CampaignEventDelivery(
                body,
                eventArgs.BasicProperties.MessageId,
                eventArgs.BasicProperties.Type,
                eventArgs.RoutingKey,
                eventArgs.Redelivered);

            try
            {
                var result = await deliveryHandler(delivery, cancellationToken);

                if (result.Disposition ==
                    CampaignEventDeliveryDispositions.Acknowledge)
                {
                    await channel.BasicAckAsync(
                        eventArgs.DeliveryTag,
                        multiple: false,
                        cancellationToken);
                    return;
                }

                if (result.Disposition ==
                    CampaignEventDeliveryDispositions.Reject)
                {
                    await channel.BasicRejectAsync(
                        eventArgs.DeliveryTag,
                        requeue: false,
                        cancellationToken);
                    return;
                }

                fatalFailure.TrySetException(
                    new InvalidOperationException(
                        $"Unknown delivery disposition: {result.Disposition}"));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Leave the delivery unacknowledged. Closing the connection below
                // returns it to RabbitMQ for redelivery.
            }
            catch (Exception exception)
            {
                // Do not ack or reject infrastructure failures. Fail the hosted
                // service so the connection closes and RabbitMQ can redeliver.
                fatalFailure.TrySetException(exception);
            }
        };

        async Task SubscribeAsync(CancellationToken subscribeCancellationToken)
        {
            await subscriptionLock.WaitAsync(subscribeCancellationToken);
            try
            {
                consumerTag = await channel.BasicConsumeAsync(
                    _options.QueueName,
                    autoAck: false,
                    consumer,
                    subscribeCancellationToken);
            }
            finally
            {
                subscriptionLock.Release();
            }
        }

        connection.RecoverySucceededAsync += async (_, _) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await SubscribeAsync(cancellationToken);
                _logger.LogInformation(
                    "RabbitMQ campaign consumer subscription recovered for queue {QueueName}.",
                    _options.QueueName);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                fatalFailure.TrySetException(exception);
            }
        };

        try
        {
            await SubscribeAsync(cancellationToken);
            _logger.LogInformation(
                "RabbitMQ campaign consumer started for queue {QueueName}.",
                _options.QueueName);

            var cancellationTask = Task.Delay(
                Timeout.InfiniteTimeSpan,
                cancellationToken);
            var completedTask = await Task.WhenAny(
                fatalFailure.Task,
                cancellationTask);

            if (completedTask == fatalFailure.Task)
            {
                await fatalFailure.Task;
            }
            else
            {
                await cancellationTask;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            await subscriptionLock.WaitAsync(CancellationToken.None);
            try
            {
                if (consumerTag is not null && channel.IsOpen)
                {
                    try
                    {
                        await channel.BasicCancelAsync(
                            consumerTag,
                            noWait: false,
                            CancellationToken.None);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogDebug(
                            exception,
                            "RabbitMQ campaign consumer cancellation failed during shutdown.");
                    }
                }
            }
            finally
            {
                subscriptionLock.Release();
                subscriptionLock.Dispose();
            }

            if (channel.IsOpen)
            {
                await channel.CloseAsync(CancellationToken.None);
            }

            if (connection.IsOpen)
            {
                await connection.CloseAsync(CancellationToken.None);
            }
        }
    }
}
