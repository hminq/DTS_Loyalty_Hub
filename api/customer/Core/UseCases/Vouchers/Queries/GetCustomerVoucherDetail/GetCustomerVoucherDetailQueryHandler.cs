using Core.Abstractions;
using Core.Exceptions;
using MediatR;

namespace Core.UseCases.Vouchers.Queries.GetCustomerVoucherDetail;

public sealed class GetCustomerVoucherDetailQueryHandler
    : IRequestHandler<GetCustomerVoucherDetailQuery, CustomerVoucherDetailResult>
{
    private readonly ICustomerVoucherRepository _repository;

    public GetCustomerVoucherDetailQueryHandler(ICustomerVoucherRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerVoucherDetailResult> Handle(
        GetCustomerVoucherDetailQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetVoucherDetailAsync(
                request.CustomerId,
                request.CustomerVoucherId,
                cancellationToken)
            ?? throw new DomainException(
                "CUSTOMER_VOUCHER_NOT_FOUND",
                DomainErrorType.NotFound);
    }
}
