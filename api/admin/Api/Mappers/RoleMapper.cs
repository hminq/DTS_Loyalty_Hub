using Api.Dtos.Requests.Roles;
using Api.Dtos.Responses.Roles;
using Core.UseCases.Roles.Commands;
using Core.UseCases.Roles.Results;

namespace Api.Mappers;

public static class RoleMapper
{
    public static CreateRoleCommand ToCommand(this CreateRoleRequestDto request)
    {
        return new CreateRoleCommand(
            request.Name,
            request.PermissionIds);
    }

    public static UpdateRoleCommand ToCommand(this UpdateRoleRequestDto request, Guid roleId)
    {
        return new UpdateRoleCommand(
            roleId,
            request.Name,
            request.PermissionIds);
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
}
