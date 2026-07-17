using Api.Dtos.Requests.AdminUsers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.AdminUsers;
using Core.UseCases.AdminUsers.Commands;
using Core.UseCases.AdminUsers.Queries;
using Core.UseCases.AdminUsers.Results;
using Core.UseCases.Common;

namespace Api.Mappers;

public static class AdminUserMapper
{
    public static GetAdminUsersQuery ToQuery(this GetAdminUsersRequestDto request)
    {
        return new GetAdminUsersQuery(
            request.Page,
            request.PageSize,
            request.Keyword,
            request.Status,
            request.RoleId);
    }

    public static CreateAdminUserCommand ToCommand(this CreateAdminUserRequestDto request, Guid? actorUserId)
    {
        return new CreateAdminUserCommand(
            request.Username!.Trim(),
            request.Email!.Trim(),
            request.Password!,
            request.FullName,
            request.PhoneNumber,
            request.RoleId,
            actorUserId);
    }

    public static UpdateAdminUserCommand ToCommand(
        this UpdateAdminUserRequestDto request,
        Guid adminId,
        Guid? actorUserId)
    {
        return new UpdateAdminUserCommand(
            adminId,
            request.Email!.Trim(),
            request.FullName,
            request.PhoneNumber,
            request.RoleId,
            actorUserId);
    }

    public static UpdateAdminUserStatusCommand ToCommand(
        this UpdateAdminUserStatusRequestDto request,
        Guid adminId,
        Guid? actorUserId)
    {
        return new UpdateAdminUserStatusCommand(
            adminId,
            request.Status!.Trim(),
            actorUserId);
    }

    public static ApiResponseDto<IReadOnlyCollection<AdminUserListItemResponseDto>> ToPagedResponseDto(
        this PagedResult<AdminUserResult> result)
    {
        return new ApiResponseDto<IReadOnlyCollection<AdminUserListItemResponseDto>>
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

    public static AdminUserListItemResponseDto ToListItemResponseDto(this AdminUserResult result)
    {
        return new AdminUserListItemResponseDto
        {
            AdminId = result.AdminId,
            Username = result.Username,
            FullName = result.FullName,
            RoleName = result.RoleName,
            Status = result.Status,
            CreatedAt = result.CreatedAt
        };
    }

    public static AdminUserResponseDto ToResponseDto(this AdminUserResult result)
    {
        return new AdminUserResponseDto
        {
            AdminId = result.AdminId,
            UserId = result.UserId,
            Username = result.Username,
            Email = result.Email,
            FullName = result.FullName,
            PhoneNumber = result.PhoneNumber,
            RoleId = result.RoleId,
            RoleName = result.RoleName,
            Role = result.Role?.ToResponseDto(),
            Status = result.Status,
            CreatedAt = result.CreatedAt
        };
    }

    private static AdminUserRoleResponseDto ToResponseDto(this AdminUserRoleResult result)
    {
        return new AdminUserRoleResponseDto
        {
            RoleId = result.RoleId,
            Name = result.Name,
            Permissions = result.Permissions
                .Select(permission => new AdminUserPermissionResponseDto
                {
                    PermissionId = permission.PermissionId,
                    Code = permission.Code,
                    Name = permission.Name,
                    GroupCode = permission.GroupCode,
                    GroupName = permission.GroupName,
                    GroupSortOrder = permission.GroupSortOrder,
                    ActionSortOrder = permission.ActionSortOrder
                })
                .ToArray()
        };
    }
}
