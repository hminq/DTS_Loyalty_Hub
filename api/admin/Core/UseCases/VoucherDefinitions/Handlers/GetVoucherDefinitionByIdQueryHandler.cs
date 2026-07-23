using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Handlers;

public sealed class GetVoucherDefinitionByIdQueryHandler
    : IRequestHandler<GetVoucherDefinitionByIdQuery, VoucherDefinitionResult>
{
    private readonly IVoucherDefinitionRepository _voucherDefinitionRepository;
    private readonly IBannerReadUrlProvider _bannerReadUrlProvider;

    public GetVoucherDefinitionByIdQueryHandler(
        IVoucherDefinitionRepository voucherDefinitionRepository,
        IBannerReadUrlProvider bannerReadUrlProvider)
    {
        _voucherDefinitionRepository = voucherDefinitionRepository;
        _bannerReadUrlProvider = bannerReadUrlProvider;
    }

    public async Task<VoucherDefinitionResult> Handle(
        GetVoucherDefinitionByIdQuery request,
        CancellationToken ct)
    {
        if (request.VoucherDefinitionId == Guid.Empty)
        {
            throw new DomainException(
                "VOUCHER_DEFINITION_ID_REQUIRED",
                DomainErrorType.Validation);
        }

        var result = await _voucherDefinitionRepository.GetByIdAsync(
                request.VoucherDefinitionId,
                ct)
            ?? throw new DomainException(
                "VOUCHER_DEFINITION_NOT_FOUND",
                DomainErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(result.BannerImageUrl))
        {
            return result;
        }

        return result with
        {
            BannerImageUrl = _bannerReadUrlProvider.CreateReadUrl(result.BannerImageUrl)
        };
    }
}
