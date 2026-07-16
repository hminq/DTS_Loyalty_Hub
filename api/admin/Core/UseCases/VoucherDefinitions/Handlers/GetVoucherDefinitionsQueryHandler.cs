using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Handlers;

public sealed class GetVoucherDefinitionsQueryHandler
    : IRequestHandler<GetVoucherDefinitionsQuery, PagedResult<VoucherDefinitionResult>>
{
    private const int MaxPageSize = 100;
    private readonly IVoucherDefinitionRepository _voucherDefinitionRepository;

    public GetVoucherDefinitionsQueryHandler(IVoucherDefinitionRepository voucherDefinitionRepository)
    {
        _voucherDefinitionRepository = voucherDefinitionRepository;
    }

    public Task<PagedResult<VoucherDefinitionResult>> Handle(
        GetVoucherDefinitionsQuery request,
        CancellationToken ct)
    {
        if (request.Page < 1)
        {
            throw new DomainException(
                "PAGE_INVALID",
                "Page must be greater than or equal to 1.",
                DomainErrorType.Validation);
        }

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
        {
            throw new DomainException(
                "PAGE_SIZE_INVALID",
                $"Page size must be between 1 and {MaxPageSize}.",
                DomainErrorType.Validation);
        }

        return _voucherDefinitionRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Keyword,
            ct);
    }
}
