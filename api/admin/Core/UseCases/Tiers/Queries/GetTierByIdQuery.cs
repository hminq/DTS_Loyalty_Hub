using Core.UseCases.Tiers.Results;
using MediatR;

namespace Core.UseCases.Tiers.Queries;

public sealed record GetTierByIdQuery(Guid TierConfigId)
    : IRequest<TierDetailResult>;
