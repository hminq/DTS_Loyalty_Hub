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

    public static GetRoleOptionsQuery ToQuery(this GetRoleOptionsRequestDto request)
    {
        return new GetRoleOptionsQuery(request.Keyword);
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

    public static RoleDetailResponseDto ToResponseDto(this RoleDetailResult result)
    {
        var permissions = result.Permissions
            .Select(permission => new RolePermissionDetailResponseDto(
                permission.PermissionId,
                permission.Code,
                permission.Name,
                permission.GroupCode,
                permission.GroupName,
                permission.ActionCode,
                permission.ActionName,
                permission.GroupSortOrder,
                permission.ActionSortOrder))
            .ToArray();

        return new RoleDetailResponseDto(
            result.RoleId,
            result.Name,
            permissions.Select(permission => permission.PermissionId).ToArray(),
            permissions,
            result.CreatedAt);
    }

    public static RoleOptionResponseDto ToResponseDto(this RoleOptionResult result)
    {
        return new RoleOptionResponseDto
        {
            RoleId = result.RoleId,
            Name = result.Name
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

    public static ApiResponseDto<IReadOnlyCollection<RoleOptionResponseDto>> ToResponseDto(
        this IReadOnlyCollection<RoleOptionResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<RoleOptionResponseDto>>
        {
            Data = result.Select(item => item.ToResponseDto()).ToArray()
        };
    }
}
