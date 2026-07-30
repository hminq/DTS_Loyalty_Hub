using Scheduler.Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Quartz;
using Scheduler.Jobs;

namespace Scheduler.Tests.Jobs;

public sealed class DispatchOutboxMessagesJobTests
{
    [Fact]
    public async Task Execute_EmptyBatch_DoesNotPublish()
    {
        var sender = new Mock<ISender>();
        var context = CreateContext();
        sender.Setup(item => item.Send(
                It.IsAny<GetDispatchableOutboxMessageIdsQuery>(),
                context.Object.CancellationToken))
            .ReturnsAsync([]);
        var job = CreateJob(sender.Object);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            It.IsAny<PublishOutboxMessageCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_FinalPartialBatch_ProcessesEverySelectedId()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var sender = new Mock<ISender>();
        var context = CreateContext();
        sender.Setup(item => item.Send(
                It.IsAny<GetDispatchableOutboxMessageIdsQuery>(),
                context.Object.CancellationToken))
            .ReturnsAsync(ids);
        sender.Setup(item => item.Send(
                It.IsAny<PublishOutboxMessageCommand>(),
                context.Object.CancellationToken))
            .ReturnsAsync((PublishOutboxMessageCommand command, CancellationToken _) =>
                new PublishOutboxMessageResult(command.EventId, OutboxDispatchOutcome.Published));
        var job = CreateJob(sender.Object);

        await job.Execute(context.Object);

        foreach (var id in ids)
        {
            sender.Verify(item => item.Send(
                new PublishOutboxMessageCommand(id),
                context.Object.CancellationToken), Times.Once);
        }
    }

    [Fact]
    public async Task Execute_UnexpectedMessageFailure_ContinuesWithLaterIds()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var sender = new Mock<ISender>();
        var context = CreateContext();
        sender.Setup(item => item.Send(
                It.IsAny<GetDispatchableOutboxMessageIdsQuery>(),
                context.Object.CancellationToken))
            .ReturnsAsync(ids);
        sender.Setup(item => item.Send(
                new PublishOutboxMessageCommand(ids[0]),
                context.Object.CancellationToken))
            .ThrowsAsync(new InvalidOperationException("save failed"));
        sender.Setup(item => item.Send(
                new PublishOutboxMessageCommand(ids[1]),
                context.Object.CancellationToken))
            .ReturnsAsync(new PublishOutboxMessageResult(
                ids[1],
                OutboxDispatchOutcome.Published));
        var job = CreateJob(sender.Object);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            new PublishOutboxMessageCommand(ids[1]),
            context.Object.CancellationToken), Times.Once);
    }

    private static DispatchOutboxMessagesJob CreateJob(ISender sender)
    {
        var provider = new ServiceCollection()
            .AddScoped(_ => sender)
            .BuildServiceProvider();

        return new DispatchOutboxMessagesJob(
            provider.GetRequiredService<IServiceScopeFactory>(),
            TimeProvider.System,
            NullLogger<DispatchOutboxMessagesJob>.Instance);
    }

    private static Mock<IJobExecutionContext> CreateContext()
    {
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(item => item.CancellationToken).Returns(CancellationToken.None);
        return context;
    }
}
