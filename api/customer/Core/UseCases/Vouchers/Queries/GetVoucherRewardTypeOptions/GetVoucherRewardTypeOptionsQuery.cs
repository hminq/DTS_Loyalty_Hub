using MediatR;

namespace Core.UseCases.Vouchers.Queries.GetVoucherRewardTypeOptions;

public sealed record GetVoucherRewardTypeOptionsQuery
    : IRequest<IReadOnlyCollection<string>>;
