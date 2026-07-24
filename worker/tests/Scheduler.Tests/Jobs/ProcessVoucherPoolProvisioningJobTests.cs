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

public sealed class ProcessVoucherPoolProvisioningJobTests
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

    [Fact]
    public async Task Execute_ImportedJob_StagesFinalPartialBatchAndCompletes()
    {
        var jobId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        var reader = new Mock<IVoucherPoolImportFileReader>();
        var context = Context();
        sender.Setup(item => item.Send(
                It.IsAny<StartOrResumeVoucherPoolGenerationCommand>(),
                context.Object.CancellationToken))
            .ReturnsAsync(new StartOrResumeVoucherPoolGenerationResult(
                true,
                jobId,
                10,
                0,
                false,
                VoucherPoolProvisioningJobTypes.Imported,
                $"voucher_defs/{Guid.NewGuid():D}/imports/{Guid.NewGuid():D}.csv"));
        sender.Setup(item => item.Send(
                It.IsAny<StageVoucherPoolImportBatchCommand>(),
                context.Object.CancellationToken))
            .ReturnsAsync((IRequest<StageVoucherPoolImportBatchResult> request, CancellationToken _) =>
            {
                var command = (StageVoucherPoolImportBatchCommand)request;
                return new StageVoucherPoolImportBatchResult(
                    command.Rows.Count,
                    command.StartProcessedCount + command.Rows.Count);
            });
        sender.Setup(item => item.Send(
                It.IsAny<CompleteVoucherPoolImportCommand>(),
                context.Object.CancellationToken))
            .Returns(Task.CompletedTask);
        reader.Setup(item => item.ReadAsync(
                It.IsAny<string>(),
                context.Object.CancellationToken))
            .Returns(ImportRows(10));

        var job = CreateJob(sender.Object, importReader: reader.Object);

        await job.Execute(context.Object);

        sender.Verify(item => item.Send(
            It.Is<StageVoucherPoolImportBatchCommand>(command =>
                command.Rows.Count == 4),
            context.Object.CancellationToken), Times.Exactly(2));
        sender.Verify(item => item.Send(
            It.Is<StageVoucherPoolImportBatchCommand>(command =>
                command.Rows.Count == 2 &&
                command.StartProcessedCount == 8),
            context.Object.CancellationToken), Times.Once);
        sender.Verify(item => item.Send(
            It.Is<CompleteVoucherPoolImportCommand>(command => command.JobId == jobId),
            context.Object.CancellationToken), Times.Once);
    }

    private static ProcessVoucherPoolProvisioningJob CreateJob(
        ISender sender,
        IVoucherPoolGenerationFailureClassifier? classifier = null,
        IVoucherPoolImportFileReader? importReader = null)
    {
        var services = new ServiceCollection().AddSingleton(sender);
        if (importReader is not null)
        {
            services.AddSingleton(importReader);
        }
        var serviceProvider = services.BuildServiceProvider();
        var options = new VoucherPoolProvisioningScheduleOptions
        {
            Cron = "0/10 * * * * ?",
            TimeZone = "Asia/Ho_Chi_Minh",
            BatchSize = 4
        };

        return new ProcessVoucherPoolProvisioningJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            classifier ?? Mock.Of<IVoucherPoolGenerationFailureClassifier>(),
            options,
            new FixedTimeProvider(Now),
            NullLogger<ProcessVoucherPoolProvisioningJob>.Instance);
    }

    private static async IAsyncEnumerable<VoucherPoolImportRawRow> ImportRows(int count)
    {
        for (var rowNumber = 1; rowNumber <= count; rowNumber++)
        {
            yield return new VoucherPoolImportRawRow(
                rowNumber,
                $"CODE-{rowNumber}");
            await Task.Yield();
        }
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
