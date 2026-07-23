using Api.Dtos.Requests.VoucherDefinitions;
using Api.Dtos.Responses;
using Api.Dtos.Responses.VoucherDefinitions;
using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Commands;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;

namespace Api.Mappers;

public static class VoucherDefinitionMapper
{
    public static CreateVoucherDefinitionCommand ToCommand(
        this CreateVoucherDefinitionRequestDto request,
        Guid? actorUserId)
    {
        return new CreateVoucherDefinitionCommand(
            request.Code,
            request.Name,
            request.Description,
            request.BannerImageUrl,
            request.RewardType,
            request.RewardValue,
            request.ValidityType,
            request.ValidFrom,
            request.ValidTo,
            request.DurationDay,
            request.GenerationType,
            request.PublishType,
            request.TotalStock,
            actorUserId);
    }

    public static GetVoucherDefinitionsQuery ToQuery(this GetVoucherDefinitionsRequestDto request)
    {
        return new GetVoucherDefinitionsQuery(
            request.Page,
            request.PageSize,
            request.Keyword,
            request.RewardType,
            request.ValidityType,
            request.PublishType);
    }

    public static VoucherDefinitionResponseDto ToResponseDto(this VoucherDefinitionResult result)
    {
        return new VoucherDefinitionResponseDto
        {
            VoucherDefinitionId = result.VoucherDefinitionId,
            Code = result.Code,
            Name = result.Name,
            Description = result.Description,
            BannerImageUrl = result.BannerImageUrl,
            RewardType = result.RewardType,
            RewardValue = result.RewardValue,
            ValidityType = result.ValidityType,
            ValidFrom = result.ValidFrom,
            ValidTo = result.ValidTo,
            DurationDay = result.DurationDay,
            GenerationType = result.GenerationType,
            PublishType = result.PublishType,
            TotalStock = result.TotalStock,
            RemainingStock = result.RemainingStock,
            CreatedAt = result.CreatedAt,
            DeletedAt = result.DeletedAt
        };
    }

    public static VoucherDefinitionListItemResponseDto ToListItemResponseDto(this VoucherDefinitionListItemResult result)
    {
        return new VoucherDefinitionListItemResponseDto
        {
            VoucherDefinitionId = result.VoucherDefinitionId,
            Code = result.Code,
            Name = result.Name,
            RewardType = result.RewardType,
            RewardValue = result.RewardValue,
            PublishType = result.PublishType,
            TotalStock = result.TotalStock,
            RemainingStock = result.RemainingStock,
            CreatedAt = result.CreatedAt,
            DeletedAt = result.DeletedAt
        };
    }

    public static ApiResponseDto<IReadOnlyCollection<VoucherDefinitionListItemResponseDto>> ToPagedResponseDto(
        this PagedResult<VoucherDefinitionListItemResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<VoucherDefinitionListItemResponseDto>>
        {
            Data = result.Items.Select(item => item.ToListItemResponseDto()).ToArray(),
            Meta = new ApiMetaDto
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        };
    }

    public static VoucherDefinitionOptionsResponseDto ToOptionsResponseDto(
        this VoucherDefinitionOptionsResult result,
        Api.Localization.VoucherDefinitionOptionLabelResolver labelResolver)
    {
        return new VoucherDefinitionOptionsResponseDto(
            RewardTypes: result.RewardTypes.Select(v => new VoucherDefinitionOptionResponseDto(v, labelResolver.ResolveRewardType(v))).ToArray(),
            ValidityTypes: result.ValidityTypes.Select(v => new VoucherDefinitionOptionResponseDto(v, labelResolver.ResolveValidityType(v))).ToArray(),
            PublishTypes: result.PublishTypes.Select(v => new VoucherDefinitionOptionResponseDto(v, labelResolver.ResolvePublishType(v))).ToArray(),
            GenerationTypes: result.GenerationTypes.Select(v => new VoucherDefinitionOptionResponseDto(v, labelResolver.ResolveGenerationType(v))).ToArray()
        );
    }
}
