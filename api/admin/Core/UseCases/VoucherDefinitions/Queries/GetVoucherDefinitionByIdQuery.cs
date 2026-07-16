using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Queries;

public sealed record GetVoucherDefinitionByIdQuery(Guid VoucherDefinitionId)
    : IRequest<VoucherDefinitionResult>;
