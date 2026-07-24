using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.CustomerVouchers.Queries.GetCustomerRedeemDetail;
using Core.UseCases.CustomerVouchers.Results;
using MediatR;

namespace Core.UseCases.CustomerVouchers.Handlers;

public sealed class GetCustomerRedeemDetailQueryHandler
    : IRequestHandler<GetCustomerRedeemDetailQuery, CustomerRedeemDetailResult>
{
    private readonly ICustomerVoucherRepository _customerVoucherRepository;

    public GetCustomerRedeemDetailQueryHandler(ICustomerVoucherRepository customerVoucherRepository)
    {
        _customerVoucherRepository = customerVoucherRepository;
    }

    public async Task<CustomerRedeemDetailResult> Handle(
        GetCustomerRedeemDetailQuery request,
        CancellationToken ct)
    {
        if (request.VoucherRedemptionId == Guid.Empty)
        {
            throw new DomainException(
                "VOUCHER_REDEMPTION_NOT_FOUND",
                DomainErrorType.NotFound);
        }

        return await _customerVoucherRepository.GetRedeemDetailAsync(
            request.VoucherRedemptionId,
            ct)
            ?? throw new DomainException(
                "VOUCHER_REDEMPTION_NOT_FOUND",
                DomainErrorType.NotFound);
    }
}
