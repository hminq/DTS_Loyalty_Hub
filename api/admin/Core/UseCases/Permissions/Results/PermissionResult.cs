namespace Core.UseCases.Permissions.Results;

public sealed record PermissionResult(
    Guid PermissionId,
    string Code,
    string Name,
    int ActionSortOrder);
