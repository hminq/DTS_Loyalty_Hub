using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Queries;

public sealed record CreateVoucherPoolImportUploadUrlQuery(
    Guid VoucherDefinitionId,
    string FileName,
    long FileSizeBytes) : IRequest<VoucherPoolImportUploadResult>;
