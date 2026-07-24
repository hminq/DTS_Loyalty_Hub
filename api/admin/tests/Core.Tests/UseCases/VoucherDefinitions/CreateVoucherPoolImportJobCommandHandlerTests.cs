using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.UseCases.AuditLogs;
using Core.UseCases.VoucherDefinitions.Commands;
using Core.UseCases.VoucherDefinitions.Handlers;
using Core.UseCases.VoucherDefinitions.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.VoucherDefinitions;

public sealed class CreateVoucherPoolImportJobCommandHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 24, 8, 0, 0, TimeSpan.Zero);
    private readonly Mock<IVoucherDefinitionRepository> _repository = new();
    private readonly Mock<IVoucherPoolProvisioningJobWriter> _jobWriter = new();
    private readonly Mock<IVoucherPoolImportObjectKeyPolicy> _keyPolicy = new();
    private readonly Mock<IAuditLogWriter> _auditWriter = new();

    [Fact]
    public async Task Handle_ValidImport_AddsPendingJobAndAudit()
    {
        var definitionId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var key = $"voucher_defs/{definitionId:D}/imports/{Guid.NewGuid():D}.csv";
        _repository.Setup(item => item.GetByIdAsync(
                definitionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImportedDefinition(definitionId));
        _keyPolicy.Setup(item => item.IsValid(definitionId, key)).Returns(true);
        _repository.Setup(item => item.HasVoucherPoolsAsync(
                definitionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(item => item.HasActiveProvisioningJobAsync(
                definitionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _jobWriter.Setup(item => item.Add(It.IsAny<VoucherPoolProvisioningJob>()))
            .Returns((VoucherPoolProvisioningJob job) => job);

        var result = await CreateHandler().Handle(
            new CreateVoucherPoolImportJobCommand(definitionId, key, actorId),
            CancellationToken.None);

        result.JobType.Should().Be(VoucherPoolProvisioningJobTypes.Imported);
        result.Status.Should().Be(VoucherPoolProvisioningJobStatuses.Pending);
        result.ExpectedCount.Should().Be(100);
        _jobWriter.Verify(item => item.Add(
            It.Is<VoucherPoolProvisioningJob>(job =>
                job.JobId != Guid.Empty &&
                job.ImportFileKey == key &&
                job.CreatedBy == actorId)), Times.Once);
        _auditWriter.Verify(item => item.Add(
            It.Is<AuditLogEntry>(entry =>
                entry.Action == AuditActions.Import &&
                entry.EntityType == AuditEntityTypes.VoucherDefinition &&
                entry.EntityId == definitionId)), Times.Once);
    }

    private CreateVoucherPoolImportJobCommandHandler CreateHandler() =>
        new(
            _repository.Object,
            _jobWriter.Object,
            _keyPolicy.Object,
            _auditWriter.Object,
            new FixedTimeProvider(Now));

    private static VoucherDefinitionResult ImportedDefinition(Guid definitionId) =>
        new(
            definitionId,
            null,
            "Imported voucher",
            null,
            null,
            VoucherRewardTypes.Fixed,
            10,
            VoucherValidityTypes.Dynamic,
            Now.UtcDateTime.AddDays(1),
            null,
            30,
            VoucherGenerationTypes.Imported,
            VoucherPublishTypes.Private,
            100,
            0,
            Now.UtcDateTime,
            null,
            null);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
