using System.Text.Json;
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
            DeletedAt = result.DeletedAt,
            PoolProvisioning = result.PoolProvisioning is null
                ? null
                : new VoucherPoolProvisioningResponseDto
                {
                    JobId = result.PoolProvisioning.JobId,
                    JobType = result.PoolProvisioning.JobType,
                    Status = result.PoolProvisioning.Status,
                    ExpectedCount = result.PoolProvisioning.ExpectedCount,
                    ProcessedCount = result.PoolProvisioning.ProcessedCount,
                    AttemptCount = result.PoolProvisioning.AttemptCount,
                    ErrorCode = result.PoolProvisioning.ErrorCode,
                    ErrorDetails = string.IsNullOrWhiteSpace(result.PoolProvisioning.ErrorDetails)
                        ? null
                        : JsonSerializer.Deserialize<JsonElement>(
                            result.PoolProvisioning.ErrorDetails),
                    CreatedAt = result.PoolProvisioning.CreatedAt,
                    StartedAt = result.PoolProvisioning.StartedAt,
                    CompletedAt = result.PoolProvisioning.CompletedAt
                }
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
        this VoucherDefinitionOptionsResult result)
    {
        return new VoucherDefinitionOptionsResponseDto(
            RewardTypes: result.RewardTypes.ToArray(),
            ValidityTypes: result.ValidityTypes.ToArray(),
            PublishTypes: result.PublishTypes.ToArray(),
            GenerationTypes: result.GenerationTypes.ToArray(),
            Constraints: new VoucherDefinitionConstraintsResponseDto(
                Core.Entities.Constants.VoucherDefinitionLimits.MaxTotalStock,
                Core.Entities.Constants.VoucherDefinitionLimits.MaxImportedTotalStock)
        );
    }

    public static VoucherImportTemplateResponseDto ToResponseDto(
        this VoucherImportTemplateResult result)
    {
        return new VoucherImportTemplateResponseDto(
            result.DownloadUrl,
            result.FileName,
            result.ExpiresAt);
    }

    public static VoucherPoolImportUploadResponseDto ToResponseDto(
        this VoucherPoolImportUploadResult result)
    {
        return new VoucherPoolImportUploadResponseDto(
            result.ObjectKey,
            result.UploadUrl,
            result.Method,
            result.ContentType,
            result.ExpiresAt,
            result.MaximumFileSizeBytes);
    }

    public static VoucherPoolProvisioningResponseDto ToResponseDto(
        this VoucherPoolProvisioningResult result)
    {
        return new VoucherPoolProvisioningResponseDto
        {
            JobId = result.JobId,
            JobType = result.JobType,
            Status = result.Status,
            ExpectedCount = result.ExpectedCount,
            ProcessedCount = result.ProcessedCount,
            AttemptCount = result.AttemptCount,
            ErrorCode = result.ErrorCode,
            ErrorDetails = null,
            CreatedAt = result.CreatedAt,
            StartedAt = result.StartedAt,
            CompletedAt = result.CompletedAt
        };
    }
}
