using Core.Entities.Constants;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Handlers;

public sealed class GetVoucherDefinitionOptionsQueryHandler : IRequestHandler<GetVoucherDefinitionOptionsQuery, VoucherDefinitionOptionsResult>
{
    public Task<VoucherDefinitionOptionsResult> Handle(GetVoucherDefinitionOptionsQuery request, CancellationToken cancellationToken)
    {
        var result = new VoucherDefinitionOptionsResult(
            RewardTypes: VoucherRewardTypes.All,
            ValidityTypes: VoucherValidityTypes.All,
            PublishTypes: VoucherPublishTypes.All,
            GenerationTypes: VoucherGenerationTypes.All
        );

        return Task.FromResult(result);
    }
}
