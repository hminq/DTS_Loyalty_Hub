using Core.UseCases.CustomerVouchers.Results;
using MediatR;

namespace Core.UseCases.CustomerVouchers.Queries.GetCustomerRedeemDetail;

public sealed record GetCustomerRedeemDetailQuery(Guid VoucherRedemptionId)
    : IRequest<CustomerRedeemDetailResult>;
