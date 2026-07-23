using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerVoucherDetail;
using Core.UseCases.CustomerVouchers.Results;
using MediatR;

namespace Core.UseCases.CustomerVouchers.Handlers;

public sealed class GetCustomerVoucherDetailQueryHandler
    : IRequestHandler<GetCustomerVoucherDetailQuery, CustomerVoucherDetailResult>
{
    private readonly ICustomerVoucherRepository _customerVoucherRepository;

    public GetCustomerVoucherDetailQueryHandler(ICustomerVoucherRepository customerVoucherRepository)
    {
        _customerVoucherRepository = customerVoucherRepository;
    }

    public async Task<CustomerVoucherDetailResult> Handle(
        GetCustomerVoucherDetailQuery request,
        CancellationToken ct)
    {
        if (request.CustomerVoucherId == Guid.Empty)
        {
            throw new DomainException(
                "CUSTOMER_VOUCHER_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        return await _customerVoucherRepository.GetVoucherDetailAsync(
            request.CustomerVoucherId,
            ct)
            ?? throw new DomainException(
                "CUSTOMER_VOUCHER_NOT_FOUND",
                DomainErrorType.NotFound);
    }
}
