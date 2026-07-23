using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Quartz;
using Scheduler.Jobs;
using Scheduler.Options;

namespace Scheduler.Tests.Jobs;

public sealed class ProcessVoucherPoolGenerationJobTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 23, 3, 4, 5, TimeSpan.Zero);

    [Fact]
    public async Task Execute_ProcessesOneJobUntilCompleted()
    {
        var jobId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        var context = Context();
        sender.Setup(item => item.Send(
                It.IsAny<StartOrResumeVoucherPoolGenerationCommand>(),
                context.Object.CancellationToken))
            .ReturnsAsync(new StartOrResumeVoucherPoolGenerationResult(
                true,
                jobId,
                6,
                0,
                false));
        sender.SetupSequence(item => item.Send(
                It.IsAny<GenerateVoucherPoolBatchCommand>(),
                context.Object.CancellationToken))
            .ReturnsAsync(new GenerateVoucherPoolBatchResult(4, 4, false))
            .ReturnsAsync(new GenerateVoucherPoolBatchResult(2, 6, true));

        var job = CreateJob(sender.Object);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            It.Is<GenerateVoucherPoolBatchCommand>(command =>
                command.JobId == jobId &&
                command.BatchSize == 4),
            context.Object.CancellationToken), Times.Exactly(2));
        sender.Verify(item => item.Send(
            It.IsAny<RecordVoucherPoolGenerationFailureCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_BatchFailure_RecordsStructuredFailure()
    {
        var jobId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        var context = Context();
        sender.Setup(item => item.Send(
                It.IsAny<StartOrResumeVoucherPoolGenerationCommand>(),
                context.Object.CancellationToken))
            .ReturnsAsync(new StartOrResumeVoucherPoolGenerationResult(
                true,
                jobId,
                10,
                4,
                false));
        sender.Setup(item => item.Send(
                It.IsAny<GenerateVoucherPoolBatchCommand>(),
                context.Object.CancellationToken))
            .ThrowsAsync(new InvalidOperationException("internal"));
        sender.Setup(item => item.Send(
                It.IsAny<RecordVoucherPoolGenerationFailureCommand>(),
                CancellationToken.None))
            .Returns(Task.CompletedTask);
        var classifier = new Mock<IVoucherPoolGenerationFailureClassifier>();
        classifier.Setup(item => item.Classify(It.IsAny<Exception>()))
            .Returns(new VoucherPoolGenerationFailure(
                VoucherPoolGenerationErrorCodes.UnexpectedError,
                true));

        var job = CreateJob(sender.Object, classifier.Object);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            It.Is<RecordVoucherPoolGenerationFailureCommand>(command =>
                command.JobId == jobId &&
                command.ErrorCode == VoucherPoolGenerationErrorCodes.UnexpectedError &&
                command.Retriable &&
                command.ErrorDetails != null &&
                command.ErrorDetails.Contains("\"ProcessedCount\":4")),
            CancellationToken.None), Times.Once);
    }

    private static ProcessVoucherPoolGenerationJob CreateJob(
        ISender sender,
        IVoucherPoolGenerationFailureClassifier? classifier = null)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(sender)
            .BuildServiceProvider();
        var options = new VoucherPoolGenerationScheduleOptions
        {
            Cron = "0/10 * * * * ?",
            TimeZone = "Asia/Ho_Chi_Minh",
            BatchSize = 4
        };

        return new ProcessVoucherPoolGenerationJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            classifier ?? Mock.Of<IVoucherPoolGenerationFailureClassifier>(),
            options,
            new FixedTimeProvider(Now),
            NullLogger<ProcessVoucherPoolGenerationJob>.Instance);
    }

    private static Mock<IJobExecutionContext> Context()
    {
        var context = new Mock<IJobExecutionContext>();
        context.SetupGet(item => item.CancellationToken)
            .Returns(CancellationToken.None);
        return context;
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
