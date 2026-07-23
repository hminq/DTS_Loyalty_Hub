using MediatR;

namespace Core.UseCases.Vouchers.Queries.GetCustomerVoucherDetail;

public sealed record GetCustomerVoucherDetailQuery(
    Guid CustomerId,
    Guid CustomerVoucherId)
    : IRequest<CustomerVoucherDetailResult>;
