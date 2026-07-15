using Api.Dtos.Responses;
using Api.Dtos.Responses.AuditLogs;
using Core.UseCases.AuditLogs.Results;
using Core.UseCases.Common;

namespace Api.Mappers;

public static class AuditLogMapper
{
    public static AuditLogResponseDto ToResponseDto(this AuditLogResult result)
    {
        return new AuditLogResponseDto
        {
            AuditLogId = result.AuditLogId,
            ActorUserId = result.ActorUserId,
            Action = result.Action,
            EntityType = result.EntityType,
            EntityId = result.EntityId,
            OldValue = result.OldValue,
            NewValue = result.NewValue,
            Metadata = result.Metadata,
            CreatedAt = result.CreatedAt
        };
    }

    public static ApiResponseDto<IReadOnlyCollection<AuditLogResponseDto>> ToPagedResponseDto(
        this PagedResult<AuditLogResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<AuditLogResponseDto>>
        {
            Data = result.Items.Select(item => item.ToResponseDto()).ToArray(),
            Meta = new ApiMetaDto
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        };
    }
}
