using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Tiers.Queries;
using Core.UseCases.Tiers.Results;
using MediatR;

namespace Core.UseCases.Tiers.Handlers;

public sealed class GetTierByIdQueryHandler
    : IRequestHandler<GetTierByIdQuery, TierDetailResult>
{
    private readonly ITierRepository _tierRepository;

    public GetTierByIdQueryHandler(ITierRepository tierRepository)
    {
        _tierRepository = tierRepository;
    }

    public async Task<TierDetailResult> Handle(
        GetTierByIdQuery request,
        CancellationToken ct)
    {
        if (request.TierConfigId == Guid.Empty)
        {
            throw new DomainException(
                "TIER_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        return await _tierRepository.GetDetailByIdAsync(request.TierConfigId, ct)
            ?? throw new DomainException(
                "TIER_NOT_FOUND",
                DomainErrorType.NotFound);
    }
}
