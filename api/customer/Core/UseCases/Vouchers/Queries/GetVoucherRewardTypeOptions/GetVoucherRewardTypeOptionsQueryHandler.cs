using Core.Entities.Constants;
using MediatR;

namespace Core.UseCases.Vouchers.Queries.GetVoucherRewardTypeOptions;

public sealed class GetVoucherRewardTypeOptionsQueryHandler
    : IRequestHandler<GetVoucherRewardTypeOptionsQuery, IReadOnlyCollection<string>>
{
    public Task<IReadOnlyCollection<string>> Handle(
        GetVoucherRewardTypeOptionsQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(VoucherRewardTypes.All);
    }
}
