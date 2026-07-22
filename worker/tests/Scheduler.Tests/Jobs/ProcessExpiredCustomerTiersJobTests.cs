using Core.Requests;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Quartz;
using Scheduler.Jobs;

namespace Scheduler.Tests.Jobs;

public sealed class ProcessExpiredCustomerTiersJobTests
{
    [Fact]
    public async Task Execute_ForwardsCurrentUtcTimeAndQuartzCancellationToCoreCommand()
    {
        var now = new DateTimeOffset(2026, 7, 22, 3, 4, 5, TimeSpan.Zero);
        var sender = new Mock<ISender>();
        var context = new Mock<IJobExecutionContext>();
        using var cancellation = new CancellationTokenSource();
        context.SetupGet(item => item.CancellationToken).Returns(cancellation.Token);
        sender.Setup(item => item.Send(
                It.IsAny<ProcessExpiredCustomerTiersCommand>(),
                cancellation.Token))
            .ReturnsAsync(2);
        var job = new ProcessExpiredCustomerTiersJob(
            sender.Object,
            new FixedTimeProvider(now),
            NullLogger<ProcessExpiredCustomerTiersJob>.Instance);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            It.Is<ProcessExpiredCustomerTiersCommand>(command => command.ProcessedAt == now.UtcDateTime),
            cancellation.Token), Times.Once);
        sender.VerifyNoOtherCalls();
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
