using Core.Abstractions;
using Core.UseCases.Tiers.Queries;
using Core.UseCases.Tiers.Results;
using MediatR;

namespace Core.UseCases.Tiers;

public sealed class GetTierConfigsQueryHandler
    : IRequestHandler<GetTierConfigsQuery, IReadOnlyCollection<TierResult>>
{
    private readonly ITierRepository _tierRepository;

    public GetTierConfigsQueryHandler(
        ITierRepository tierRepository)
    {
        _tierRepository = tierRepository;
    }

    public Task<IReadOnlyCollection<TierResult>> Handle(
        GetTierConfigsQuery request,
        CancellationToken ct)
    {
        return _tierRepository.GetListAsync(ct);
    }
}