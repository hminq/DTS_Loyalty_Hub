using System.Text.Json;
using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.VoucherDefinitions.Commands;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Handlers;

public sealed class CreateVoucherPoolImportJobCommandHandler
    : IRequestHandler<CreateVoucherPoolImportJobCommand, VoucherPoolProvisioningResult>
{
    private readonly IVoucherDefinitionRepository _repository;
    private readonly IVoucherPoolProvisioningJobWriter _jobWriter;
    private readonly IVoucherPoolImportObjectKeyPolicy _objectKeyPolicy;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly TimeProvider _timeProvider;

    public CreateVoucherPoolImportJobCommandHandler(
        IVoucherDefinitionRepository repository,
        IVoucherPoolProvisioningJobWriter jobWriter,
        IVoucherPoolImportObjectKeyPolicy objectKeyPolicy,
        IAuditLogWriter auditLogWriter,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _jobWriter = jobWriter;
        _objectKeyPolicy = objectKeyPolicy;
        _auditLogWriter = auditLogWriter;
        _timeProvider = timeProvider;
    }

    public async Task<VoucherPoolProvisioningResult> Handle(
        CreateVoucherPoolImportJobCommand request,
        CancellationToken ct)
    {
        var definition = await _repository.GetByIdAsync(request.VoucherDefinitionId, ct)
            ?? throw new DomainException(
                "VOUCHER_DEFINITION_NOT_FOUND",
                DomainErrorType.NotFound);

        if (definition.DeletedAt is not null ||
            definition.PublishType != VoucherPublishTypes.Private ||
            definition.GenerationType != VoucherGenerationTypes.Imported ||
            definition.TotalStock > VoucherDefinitionLimits.MaxImportedTotalStock ||
            definition.RemainingStock != 0)
        {
            throw new DomainException(
                "VOUCHER_DEFINITION_IMPORT_NOT_ALLOWED",
                DomainErrorType.Conflict);
        }

        if (!_objectKeyPolicy.IsValid(request.VoucherDefinitionId, request.ImportFileKey))
        {
            throw new DomainException(
                "VOUCHER_POOL_IMPORT_FILE_KEY_INVALID",
                DomainErrorType.Validation);
        }

        if (await _repository.HasVoucherPoolsAsync(request.VoucherDefinitionId, ct) ||
            definition.PoolProvisioning?.Status == VoucherPoolProvisioningJobStatuses.Completed)
        {
            throw new DomainException(
                "VOUCHER_POOL_ALREADY_PROVISIONED",
                DomainErrorType.Conflict);
        }

        if (await _repository.HasActiveProvisioningJobAsync(request.VoucherDefinitionId, ct))
        {
            throw new DomainException(
                "VOUCHER_POOL_IMPORT_ALREADY_ACTIVE",
                DomainErrorType.Conflict);
        }

        var createdAt = _timeProvider.GetUtcNow().UtcDateTime;
        var job = VoucherPoolProvisioningJob.CreateImported(
            request.VoucherDefinitionId,
            request.ImportFileKey,
            definition.TotalStock,
            request.ActorUserId,
            createdAt);
        _jobWriter.Add(job);

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Import,
            AuditEntityTypes.VoucherDefinition,
            request.VoucherDefinitionId,
            null,
            JsonSerializer.Serialize(new
            {
                jobId = job.JobId,
                importFileKey = job.ImportFileKey,
                expectedCount = job.ExpectedCount,
                status = job.Status
            }),
            null));

        return new VoucherPoolProvisioningResult(
            job.JobId,
            job.JobType,
            job.Status,
            job.ExpectedCount,
            job.ProcessedCount,
            job.AttemptCount,
            null,
            null,
            job.CreatedAt,
            null,
            null);
    }
}
