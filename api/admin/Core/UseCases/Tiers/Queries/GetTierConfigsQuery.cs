using Core.UseCases.Tiers.Results;
using MediatR;

namespace Core.UseCases.Tiers.Queries;

public sealed record GetTierConfigsQuery()
    : IRequest<IReadOnlyCollection<TierListItemResult>>;
