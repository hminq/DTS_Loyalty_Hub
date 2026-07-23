using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Queries;

public sealed record GetVoucherDefinitionOptionsQuery : IRequest<VoucherDefinitionOptionsResult>;
