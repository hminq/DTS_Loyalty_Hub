namespace Core.UseCases.Permissions.Results;

public sealed record PermissionGroupResult(
    string GroupCode,
    string GroupName,
    int GroupSortOrder,
    IReadOnlyCollection<PermissionResult> Permissions);
