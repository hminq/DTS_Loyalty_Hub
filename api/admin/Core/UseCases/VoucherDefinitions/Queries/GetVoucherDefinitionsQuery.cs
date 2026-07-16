using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Queries;

public sealed record GetVoucherDefinitionsQuery(
    int Page,
    int PageSize,
    string? Keyword) : IRequest<PagedResult<VoucherDefinitionResult>>;
