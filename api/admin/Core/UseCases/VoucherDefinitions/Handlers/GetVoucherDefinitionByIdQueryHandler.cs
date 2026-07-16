using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Handlers;

public sealed class GetVoucherDefinitionByIdQueryHandler
    : IRequestHandler<GetVoucherDefinitionByIdQuery, VoucherDefinitionResult>
{
    private readonly IVoucherDefinitionRepository _voucherDefinitionRepository;

    public GetVoucherDefinitionByIdQueryHandler(
        IVoucherDefinitionRepository voucherDefinitionRepository)
    {
        _voucherDefinitionRepository = voucherDefinitionRepository;
    }

    public async Task<VoucherDefinitionResult> Handle(
        GetVoucherDefinitionByIdQuery request,
        CancellationToken ct)
    {
        if (request.VoucherDefinitionId == Guid.Empty)
        {
            throw new DomainException(
                "VOUCHER_DEFINITION_ID_REQUIRED",
                "Voucher definition id is required.",
                DomainErrorType.Validation);
        }

        return await _voucherDefinitionRepository.GetByIdAsync(
                request.VoucherDefinitionId,
                ct)
            ?? throw new DomainException(
                "VOUCHER_DEFINITION_NOT_FOUND",
                "Voucher definition does not exist.",
                DomainErrorType.NotFound);
    }
}
