using Api.Dtos.Responses.Permissions;
using Core.UseCases.Permissions.Results;

namespace Api.Mappers;

public static class PermissionMapper
{
    public static IReadOnlyCollection<PermissionGroupResponseDto> ToResponseDto(
        this IReadOnlyCollection<PermissionGroupResult> results)
    {
        return results
            .Select(group => new PermissionGroupResponseDto
            {
                GroupCode = group.GroupCode,
                GroupName = group.GroupName,
                GroupSortOrder = group.GroupSortOrder,
                Permissions = group.Permissions
                    .Select(permission => new PermissionResponseDto
                    {
                        PermissionId = permission.PermissionId,
                        Code = permission.Code,
                        Name = permission.Name,
                        ActionCode = permission.ActionCode,
                        ActionName = permission.ActionName,
                        ActionSortOrder = permission.ActionSortOrder
                    })
                    .ToArray()
            })
            .ToArray();
    }
}
