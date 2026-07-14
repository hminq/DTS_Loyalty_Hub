namespace Core.UseCases.Roles.Results;

public sealed record RoleResult(
    Guid RoleId,
    string Name,
    IReadOnlyCollection<Guid> PermissionIds,
    DateTime CreatedAt);
