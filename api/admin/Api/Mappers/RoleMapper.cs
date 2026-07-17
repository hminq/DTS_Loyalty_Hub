using Api.Dtos.Requests.Roles;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Roles;
using Core.UseCases.Common;
using Core.UseCases.Roles.Commands;
using Core.UseCases.Roles.Queries;
using Core.UseCases.Roles.Results;

namespace Api.Mappers;

public static class RoleMapper
{
    public static GetRolesQuery ToQuery(this GetRolesRequestDto request)
    {
        return new GetRolesQuery(
            request.Page,
            request.PageSize,
            request.Keyword);
    }

    public static CreateRoleCommand ToCommand(this CreateRoleRequestDto request, Guid? actorUserId)
    {
        return new CreateRoleCommand(
            request.Name,
            request.PermissionIds,
            actorUserId);
    }

    public static UpdateRoleCommand ToCommand(this UpdateRoleRequestDto request, Guid roleId, Guid? actorUserId)
    {
        return new UpdateRoleCommand(
            roleId,
            request.Name,
            request.PermissionIds,
            actorUserId);
    }

    public static RoleResponseDto ToResponseDto(this RoleResult result)
    {
        return new RoleResponseDto
        {
            RoleId = result.RoleId,
            Name = result.Name,
            PermissionIds = result.PermissionIds,
            CreatedAt = result.CreatedAt
        };
    }

    public static ApiResponseDto<IReadOnlyCollection<RoleResponseDto>> ToPagedResponseDto(
        this PagedResult<RoleResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<RoleResponseDto>>
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
