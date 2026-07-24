using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Handlers;

public sealed class CreateVoucherPoolImportUploadUrlQueryHandler
    : IRequestHandler<CreateVoucherPoolImportUploadUrlQuery, VoucherPoolImportUploadResult>
{
    private readonly IVoucherDefinitionRepository _repository;
    private readonly IVoucherPoolImportUploadUrlProvider _uploadProvider;

    public CreateVoucherPoolImportUploadUrlQueryHandler(
        IVoucherDefinitionRepository repository,
        IVoucherPoolImportUploadUrlProvider uploadProvider)
    {
        _repository = repository;
        _uploadProvider = uploadProvider;
    }

    public async Task<VoucherPoolImportUploadResult> Handle(
        CreateVoucherPoolImportUploadUrlQuery request,
        CancellationToken ct)
    {
        var definition = await _repository.GetByIdAsync(request.VoucherDefinitionId, ct)
            ?? throw new DomainException(
                "VOUCHER_DEFINITION_NOT_FOUND",
                DomainErrorType.NotFound);

        EnsureImportAllowed(definition);

        if (definition.TotalStock > VoucherDefinitionLimits.MaxImportedTotalStock)
        {
            throw new DomainException(
                "VOUCHER_DEFINITION_IMPORT_NOT_ALLOWED",
                DomainErrorType.Conflict);
        }

        if (string.IsNullOrWhiteSpace(request.FileName) ||
            !request.FileName.Trim().EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(
                "VOUCHER_POOL_IMPORT_FILE_TYPE_INVALID",
                DomainErrorType.Validation);
        }

        if (request.FileSizeBytes <= 0 ||
            request.FileSizeBytes > _uploadProvider.MaximumFileSizeBytes)
        {
            throw new DomainException(
                "VOUCHER_POOL_IMPORT_FILE_SIZE_INVALID",
                DomainErrorType.Validation);
        }

        if (await _repository.HasVoucherPoolsAsync(request.VoucherDefinitionId, ct))
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

        var upload = _uploadProvider.CreateUpload(request.VoucherDefinitionId);
        return new VoucherPoolImportUploadResult(
            upload.ObjectKey,
            upload.UploadUrl,
            upload.Method,
            upload.ContentType,
            upload.ExpiresAt,
            _uploadProvider.MaximumFileSizeBytes);
    }

    private static void EnsureImportAllowed(VoucherDefinitionResult definition)
    {
        if (definition.DeletedAt is not null ||
            definition.PublishType != VoucherPublishTypes.Private ||
            definition.GenerationType != VoucherGenerationTypes.Imported ||
            definition.RemainingStock != 0 ||
            definition.PoolProvisioning?.Status == VoucherPoolProvisioningJobStatuses.Completed)
        {
            throw new DomainException(
                "VOUCHER_DEFINITION_IMPORT_NOT_ALLOWED",
                DomainErrorType.Conflict);
        }
    }
}
