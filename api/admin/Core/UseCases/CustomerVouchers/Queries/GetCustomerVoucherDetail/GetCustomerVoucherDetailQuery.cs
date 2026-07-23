using Core.UseCases.CustomerVouchers.Results;
using MediatR;

namespace Core.UseCases.CustomerVouchers.Queries.GetCustomerVoucherDetail;

public sealed record GetCustomerVoucherDetailQuery(Guid CustomerVoucherId)
    : IRequest<CustomerVoucherDetailResult>;
