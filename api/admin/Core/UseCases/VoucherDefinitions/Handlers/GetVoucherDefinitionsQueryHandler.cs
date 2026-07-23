using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Handlers;

public sealed class GetVoucherDefinitionsQueryHandler
    : IRequestHandler<GetVoucherDefinitionsQuery, PagedResult<VoucherDefinitionListItemResult>>
{
    private const int MaxPageSize = 100;
    private readonly IVoucherDefinitionRepository _voucherDefinitionRepository;

    public GetVoucherDefinitionsQueryHandler(IVoucherDefinitionRepository voucherDefinitionRepository)
    {
        _voucherDefinitionRepository = voucherDefinitionRepository;
    }

    public Task<PagedResult<VoucherDefinitionListItemResult>> Handle(
        GetVoucherDefinitionsQuery request,
        CancellationToken ct)
    {
        if (request.Page < 1)
        {
            throw new DomainException(
                "PAGE_INVALID",
                DomainErrorType.Validation);
        }

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
        {
            throw new DomainException(
                "PAGE_SIZE_INVALID",
                DomainErrorType.Validation);
        }

        string? rewardType = string.IsNullOrWhiteSpace(request.RewardType) ? null : request.RewardType;
        if (rewardType is not null && !VoucherRewardTypes.IsDefined(rewardType))
        {
            throw new DomainException("VOUCHER_REWARD_TYPE_INVALID", DomainErrorType.Validation);
        }
        rewardType = rewardType is not null ? VoucherRewardTypes.Normalize(rewardType) : null;
        
        string? validityType = string.IsNullOrWhiteSpace(request.ValidityType) ? null : request.ValidityType;
        if (validityType is not null && !VoucherValidityTypes.IsDefined(validityType))
        {
            throw new DomainException("VOUCHER_VALIDITY_TYPE_INVALID", DomainErrorType.Validation);
        }
        validityType = validityType is not null ? VoucherValidityTypes.Normalize(validityType) : null;
        
        string? publishType = string.IsNullOrWhiteSpace(request.PublishType) ? null : request.PublishType;
        if (publishType is not null && !VoucherPublishTypes.IsDefined(publishType))
        {
            throw new DomainException("VOUCHER_PUBLISH_TYPE_INVALID", DomainErrorType.Validation);
        }
        publishType = publishType is not null ? VoucherPublishTypes.Normalize(publishType) : null;

        return _voucherDefinitionRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Keyword,
            rewardType,
            validityType,
            publishType,
            ct);
    }
}
