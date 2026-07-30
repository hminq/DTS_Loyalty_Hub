using Scheduler.Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Quartz;
using Scheduler.Jobs;
using Scheduler.Options;
using Xunit;

namespace Scheduler.Tests.Jobs;

public sealed class ProcessCampaignSessionLifecycleJobTests
{
    [Fact]
    public async Task Execute_ForwardsCurrentUtcTimeBatchSizeAndQuartzCancellation()
    {
        var now = new DateTimeOffset(2026, 7, 28, 12, 0, 0, TimeSpan.Zero);
        var sender = new Mock<ISender>();
        var context = new Mock<IJobExecutionContext>();
        using var cancellation = new CancellationTokenSource();
        context.SetupGet(item => item.CancellationToken).Returns(cancellation.Token);
        sender.Setup(item => item.Send(
                It.IsAny<ProcessCampaignSessionLifecycleBatchCommand>(),
                cancellation.Token))
            .ReturnsAsync(new ProcessCampaignSessionLifecycleBatchResult(10, 5, 10, 5));
        sender.Setup(item => item.Send(
                It.IsAny<EndEligibleCampaignsBatchCommand>(),
                cancellation.Token))
            .ReturnsAsync(new EndEligibleCampaignsBatchResult(2, 2));

        using var serviceProvider = new ServiceCollection()
            .AddSingleton(sender.Object)
            .BuildServiceProvider();
        var options = new CampaignSessionLifecycleScheduleOptions
        {
            Cron = "0 * * * * ?",
            BatchSize = 100,
        };
        var job = new ProcessCampaignSessionLifecycleJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            options,
            new FixedTimeProvider(now),
            NullLogger<ProcessCampaignSessionLifecycleJob>.Instance);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            It.Is<ProcessCampaignSessionLifecycleBatchCommand>(command =>
                command.ProcessedAt == now.UtcDateTime &&
                command.BatchSize == options.BatchSize),
            cancellation.Token), Times.Once);
        sender.Verify(item => item.Send(
            It.Is<EndEligibleCampaignsBatchCommand>(command =>
                command.ProcessedAt == now.UtcDateTime &&
                command.BatchSize == options.BatchSize),
            cancellation.Token), Times.Once);
        sender.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_WhenBatchSizeIsReached_LoopsUntilPartialBatch()
    {
        var now = new DateTimeOffset(2026, 7, 28, 12, 0, 0, TimeSpan.Zero);
        var sender = new Mock<ISender>();
        var context = new Mock<IJobExecutionContext>();
        using var cancellation = new CancellationTokenSource();
        context.SetupGet(item => item.CancellationToken).Returns(cancellation.Token);

        sender.SetupSequence(item => item.Send(
                It.IsAny<ProcessCampaignSessionLifecycleBatchCommand>(),
                cancellation.Token))
            .ReturnsAsync(new ProcessCampaignSessionLifecycleBatchResult(100, 0, 100, 0))
            .ReturnsAsync(new ProcessCampaignSessionLifecycleBatchResult(20, 0, 20, 0));

        sender.SetupSequence(item => item.Send(
                It.IsAny<EndEligibleCampaignsBatchCommand>(),
                cancellation.Token))
            .ReturnsAsync(new EndEligibleCampaignsBatchResult(100, 100))
            .ReturnsAsync(new EndEligibleCampaignsBatchResult(5, 5));

        using var serviceProvider = new ServiceCollection()
            .AddSingleton(sender.Object)
            .BuildServiceProvider();
        var options = new CampaignSessionLifecycleScheduleOptions
        {
            Cron = "0 * * * * ?",
            BatchSize = 100,
        };
        var job = new ProcessCampaignSessionLifecycleJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            options,
            new FixedTimeProvider(now),
            NullLogger<ProcessCampaignSessionLifecycleJob>.Instance);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            It.IsAny<ProcessCampaignSessionLifecycleBatchCommand>(),
            cancellation.Token), Times.Exactly(2));
        sender.Verify(item => item.Send(
            It.IsAny<EndEligibleCampaignsBatchCommand>(),
            cancellation.Token), Times.Exactly(2));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
