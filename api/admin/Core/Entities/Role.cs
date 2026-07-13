using Core.Exceptions;

namespace Core.Entities;

public class Role
{
    private const int MaxNameLength = 100;

    private readonly HashSet<Guid> _permissionIds;

    private Role(
        Guid roleId,
        string name,
        IEnumerable<Guid> permissionIds,
        DateTime createdAt)
    {
        RoleId = roleId;
        Name = name;
        _permissionIds = permissionIds.ToHashSet();
        CreatedAt = createdAt;
    }

    public Guid RoleId { get; private set; }

    public string Name { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Guid> PermissionIds => _permissionIds;

    public static Role Create(string name)
    {
        ValidateName(name);

        return new Role(
            Guid.NewGuid(),
            name.Trim(),
            [],
            DateTime.UtcNow);
    }

    public static Role Restore(
        Guid roleId,
        string name,
        IEnumerable<Guid> permissionIds,
        DateTime createdAt)
    {
        if (roleId == Guid.Empty)
        {
            throw ValidationError("ROLE_ID_REQUIRED", "Role id is required.");
        }

        ValidateName(name);
        ValidatePermissionIds(permissionIds);

        return new Role(
            roleId,
            name.Trim(),
            permissionIds,
            createdAt);
    }

    public void Rename(string name)
    {
        ValidateName(name);

        Name = name.Trim();
    }

    public void GrantPermission(Permission permission)
    {
        if (permission is null)
        {
            throw ValidationError("PERMISSION_REQUIRED", "Permission is required.");
        }

        GrantPermission(permission.PermissionId);
    }

    public void GrantPermission(Guid permissionId)
    {
        ValidatePermissionId(permissionId);

        if (!_permissionIds.Add(permissionId))
        {
            throw new DomainException(
                "ROLE_PERMISSION_ALREADY_GRANTED",
                "This role already has the permission.",
                DomainErrorType.Conflict);
        }
    }

    public void RevokePermission(Guid permissionId)
    {
        ValidatePermissionId(permissionId);

        if (!_permissionIds.Remove(permissionId))
        {
            throw new DomainException(
                "ROLE_PERMISSION_NOT_GRANTED",
                "This role does not have the permission.",
                DomainErrorType.Conflict);
        }
    }

    public void ReplacePermissions(IEnumerable<Guid> permissionIds)
    {
        ValidatePermissionIds(permissionIds);

        _permissionIds.Clear();

        foreach (var permissionId in permissionIds)
        {
            _permissionIds.Add(permissionId);
        }
    }

    public bool HasPermission(Guid permissionId)
    {
        return permissionId != Guid.Empty && _permissionIds.Contains(permissionId);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw ValidationError("ROLE_NAME_REQUIRED", "Role name is required.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw ValidationError("ROLE_NAME_TOO_LONG", "Role name is too long.");
        }
    }

    private static void ValidatePermissionIds(IEnumerable<Guid> permissionIds)
    {
        if (permissionIds is null)
        {
            throw ValidationError("ROLE_PERMISSION_IDS_REQUIRED", "Permission ids are required.");
        }

        foreach (var permissionId in permissionIds)
        {
            ValidatePermissionId(permissionId);
        }
    }

    private static void ValidatePermissionId(Guid permissionId)
    {
        if (permissionId == Guid.Empty)
        {
            throw ValidationError("PERMISSION_ID_REQUIRED", "Permission id is required.");
        }
    }

    private static DomainException ValidationError(string errorCode, string message)
    {
        return new DomainException(errorCode, message, DomainErrorType.Validation);
    }
}
