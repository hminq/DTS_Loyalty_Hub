using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Quartz;
using Scheduler.Jobs;
using Scheduler.Options;

namespace Scheduler.Tests.Jobs;

public sealed class ProcessExpiredCustomerTiersJobTests
{
    [Fact]
    public async Task Execute_ForwardsCurrentUtcTimeBatchSizeAndQuartzCancellation()
    {
        var now = new DateTimeOffset(2026, 7, 22, 3, 4, 5, TimeSpan.Zero);
        var sender = new Mock<ISender>();
        var context = new Mock<IJobExecutionContext>();
        using var cancellation = new CancellationTokenSource();
        context.SetupGet(item => item.CancellationToken).Returns(cancellation.Token);
        sender.Setup(item => item.Send(
                It.IsAny<ProcessExpiredCustomerTierBatchCommand>(),
                cancellation.Token))
            .ReturnsAsync(new ProcessExpiredCustomerTierBatchResult(2, 2));

        using var serviceProvider = new ServiceCollection()
            .AddSingleton(sender.Object)
            .BuildServiceProvider();
        var options = new TierExpirationScheduleOptions
        {
            Cron = "0 0 2 * * ?",
            TimeZone = "Asia/Ho_Chi_Minh",
            BatchSize = 100,
        };
        var job = new ProcessExpiredCustomerTiersJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            options,
            new FixedTimeProvider(now),
            NullLogger<ProcessExpiredCustomerTiersJob>.Instance);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            It.Is<ProcessExpiredCustomerTierBatchCommand>(command =>
                command.ProcessedAt == now.UtcDateTime &&
                command.BatchSize == options.BatchSize),
            cancellation.Token), Times.Once);
        sender.VerifyNoOtherCalls();
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
