using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Handlers;
using Core.Requests;
using FluentAssertions;
using Moq;

namespace Core.Tests.Handlers;

public sealed class VoucherPoolGenerationCommandHandlerTests
{
    private static readonly DateTime ProcessedAt =
        new(2026, 7, 23, 1, 2, 3, DateTimeKind.Utc);
    private readonly Mock<IVoucherPoolProvisioningRepository> _repository = new();
    private readonly Mock<IVoucherPoolMutationStore> _mutationStore = new();
    private readonly Mock<IVoucherCodeGenerator> _codeGenerator = new();
    private readonly Mock<IVoucherPoolImportStore> _importStore = new();

    [Fact]
    public async Task Start_ValidPendingJob_MarksProcessingAndIncrementsAttempt()
    {
        var job = Job(expectedCount: 10, processedCount: 4);
        _repository.Setup(store => store.GetNextProvisioningJobAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _mutationStore.Setup(store => store.CountPoolsAsync(
                job.VoucherDefinitionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job.ProcessedCount);

        var result = await new StartOrResumeVoucherPoolGenerationCommandHandler(
                _repository.Object,
                _mutationStore.Object)
            .Handle(
                new StartOrResumeVoucherPoolGenerationCommand(ProcessedAt),
                CancellationToken.None);

        result.HasWork.Should().BeTrue();
        result.JobId.Should().Be(job.JobId);
        _repository.Verify(store => store.MarkStartedAsync(
            job.JobId,
            1,
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Start_PoolCountDoesNotMatchProgress_MarksJobFailed()
    {
        var job = Job(expectedCount: 10, processedCount: 4);
        _repository.Setup(store => store.GetNextProvisioningJobAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _mutationStore.Setup(store => store.CountPoolsAsync(
                job.VoucherDefinitionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var result = await new StartOrResumeVoucherPoolGenerationCommandHandler(
                _repository.Object,
                _mutationStore.Object)
            .Handle(
                new StartOrResumeVoucherPoolGenerationCommand(ProcessedAt),
                CancellationToken.None);

        result.HasWork.Should().BeFalse();
        result.Failed.Should().BeTrue();
        _repository.Verify(store => store.RecordFailureAsync(
            job.JobId,
            VoucherPoolProvisioningJobStatuses.Failed,
            VoucherPoolGenerationErrorCodes.StateInvalid,
            null,
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateBatch_RemainingExceedsBatch_AddsBoundedBatch()
    {
        var job = Job(expectedCount: 10, processedCount: 4) with
        {
            Status = VoucherPoolProvisioningJobStatuses.Processing
        };
        SetupGeneration(job);

        var result = await CreateBatchHandler().Handle(
            new GenerateVoucherPoolBatchCommand(job.JobId, 4, ProcessedAt),
            CancellationToken.None);

        result.Should().Be(new GenerateVoucherPoolBatchResult(4, 8, false));
        _mutationStore.Verify(store => store.BulkInsertPoolsAsync(
            It.Is<IReadOnlyCollection<VoucherPoolMutation>>(pools =>
                pools.Count == 4 &&
                pools.All(pool =>
                    pool.VoucherDefinitionId == job.VoucherDefinitionId &&
                    pool.Status == VoucherPoolStatuses.Available &&
                    pool.CreatedAt == ProcessedAt)),
            It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(store => store.ApplyProgressAsync(
            job.JobId,
            8,
            false,
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateBatch_FinalPartialBatch_CompletesJob()
    {
        var job = Job(expectedCount: 10, processedCount: 8) with
        {
            Status = VoucherPoolProvisioningJobStatuses.Processing
        };
        SetupGeneration(job);

        var result = await CreateBatchHandler().Handle(
            new GenerateVoucherPoolBatchCommand(job.JobId, 4, ProcessedAt),
            CancellationToken.None);

        result.Should().Be(new GenerateVoucherPoolBatchResult(2, 10, true));
        _repository.Verify(store => store.ApplyProgressAsync(
            job.JobId,
            10,
            true,
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateBatch_GeneratorKeepsReturningDuplicate_StopsWithCollisionError()
    {
        var job = Job(expectedCount: 2, processedCount: 0) with
        {
            Status = VoucherPoolProvisioningJobStatuses.Processing
        };
        _repository.Setup(store => store.GetJobAsync(job.JobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _codeGenerator.Setup(generator => generator.Generate())
            .Returns("VCH-00000000000000000000000000000000");

        var action = () => CreateBatchHandler().Handle(
            new GenerateVoucherPoolBatchCommand(job.JobId, 2, ProcessedAt),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<VoucherPoolGenerationException>();
        exception.Which.ErrorCode.Should().Be(
            VoucherPoolGenerationErrorCodes.CodeCollision);
        _mutationStore.Verify(store => store.BulkInsertPoolsAsync(
            It.IsAny<IReadOnlyCollection<VoucherPoolMutation>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordFailure_RetriableBeforeMaxAttempts_RequeuesJob()
    {
        var job = Job(expectedCount: 10, processedCount: 4) with
        {
            Status = VoucherPoolProvisioningJobStatuses.Processing,
            AttemptCount = 1
        };
        _repository.Setup(store => store.GetJobAsync(job.JobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        await new RecordVoucherPoolGenerationFailureCommandHandler(_repository.Object)
            .Handle(
                new RecordVoucherPoolGenerationFailureCommand(
                    job.JobId,
                    VoucherPoolGenerationErrorCodes.DatabaseError,
                    "{}",
                    true,
                    ProcessedAt),
                CancellationToken.None);

        _repository.Verify(store => store.RecordFailureAsync(
            job.JobId,
            VoucherPoolProvisioningJobStatuses.Pending,
            VoucherPoolGenerationErrorCodes.DatabaseError,
            "{}",
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StageImportBatch_TrimsCodesAndUpdatesProgress()
    {
        var job = ImportedJob(expectedCount: 3, processedCount: 0);
        _repository.Setup(store => store.GetJobAsync(
                job.JobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _importStore.Setup(store => store.FindStagedCodesAsync(
                job.JobId,
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        var result = await new StageVoucherPoolImportBatchCommandHandler(
                _repository.Object,
                _importStore.Object)
            .Handle(
                new StageVoucherPoolImportBatchCommand(
                    job.JobId,
                    0,
                    [
                        new VoucherPoolImportRawRow(1, " CODE-1 "),
                        new VoucherPoolImportRawRow(2, "CODE-2")
                    ],
                    ProcessedAt),
                CancellationToken.None);

        result.Should().Be(new StageVoucherPoolImportBatchResult(2, 2));
        _importStore.Verify(store => store.BulkInsertStagingAsync(
            It.Is<IReadOnlyCollection<VoucherPoolImportMutation>>(rows =>
                rows.Count == 2 &&
                rows.Any(row => row.VoucherCode == "CODE-1") &&
                rows.All(row => row.VoucherPoolId != Guid.Empty)),
            It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(store => store.ApplyProgressAsync(
            job.JobId,
            2,
            false,
            ProcessedAt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StageImportBatch_DuplicateInBatch_ThrowsStableError()
    {
        var job = ImportedJob(expectedCount: 2, processedCount: 0);
        _repository.Setup(store => store.GetJobAsync(
                job.JobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        var action = () => new StageVoucherPoolImportBatchCommandHandler(
                _repository.Object,
                _importStore.Object)
            .Handle(
                new StageVoucherPoolImportBatchCommand(
                    job.JobId,
                    0,
                    [
                        new VoucherPoolImportRawRow(1, "SAME"),
                        new VoucherPoolImportRawRow(2, "SAME")
                    ],
                    ProcessedAt),
                CancellationToken.None);

        var exception = await action.Should().ThrowAsync<VoucherPoolImportException>();
        exception.Which.ErrorCode.Should().Be(
            VoucherPoolGenerationErrorCodes.ImportDuplicateInFile);
        exception.Which.RowNumber.Should().Be(2);
    }

    private void SetupGeneration(VoucherPoolGenerationJob job)
    {
        var sequence = 0;
        _repository.Setup(store => store.GetJobAsync(job.JobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _mutationStore.Setup(store => store.FindExistingCodesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());
        _mutationStore.Setup(store => store.BulkInsertPoolsAsync(
                It.IsAny<IReadOnlyCollection<VoucherPoolMutation>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _codeGenerator.Setup(generator => generator.Generate())
            .Returns(() => $"VCH-{sequence++:D32}");
    }

    private GenerateVoucherPoolBatchCommandHandler CreateBatchHandler() =>
        new(_repository.Object, _mutationStore.Object, _codeGenerator.Object);

    private static VoucherPoolGenerationJob Job(
        int expectedCount,
        int processedCount) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            VoucherPoolProvisioningJobTypes.AutoGenerated,
            expectedCount,
            processedCount,
            VoucherPoolProvisioningJobStatuses.Pending,
            0,
            expectedCount,
            0,
            VoucherPoolProvisioningJobTypes.AutoGenerated,
            "PRIVATE",
            null);

    private static VoucherPoolGenerationJob ImportedJob(
        int expectedCount,
        int processedCount) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            VoucherPoolProvisioningJobTypes.Imported,
            expectedCount,
            processedCount,
            VoucherPoolProvisioningJobStatuses.Processing,
            1,
            expectedCount,
            0,
            VoucherPoolProvisioningJobTypes.Imported,
            "PRIVATE",
            ProcessedAt,
            $"voucher_defs/{Guid.NewGuid():D}/imports/{Guid.NewGuid():D}.csv");
}
