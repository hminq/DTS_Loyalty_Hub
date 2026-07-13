using Core.Entities.Constants;
using Core.Exceptions;

namespace Core.Entities;

public class Permission
{
    private const int MaxCodeLength = 100;
    private const int MaxNameLength = 100;
    private const int MaxGroupCodeLength = 50;
    private const int MaxGroupNameLength = 100;

    private Permission(
        Guid permissionId,
        string code,
        string name,
        string groupCode,
        string groupName,
        int groupSortOrder,
        int actionSortOrder,
        DateTime createdAt)
    {
        PermissionId = permissionId;
        Code = code;
        Name = name;
        GroupCode = groupCode;
        GroupName = groupName;
        GroupSortOrder = groupSortOrder;
        ActionSortOrder = actionSortOrder;
        CreatedAt = createdAt;
    }

    public Guid PermissionId { get; private set; }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string GroupCode { get; private set; }

    public string GroupName { get; private set; }

    public int GroupSortOrder { get; private set; }

    public int ActionSortOrder { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Permission Create(
        string code,
        string name,
        string groupCode,
        string groupName,
        int groupSortOrder,
        int actionSortOrder)
    {
        Validate(code, name, groupCode, groupName, groupSortOrder, actionSortOrder);
        ValidateDefinedCode(code);

        return new Permission(
            Guid.NewGuid(),
            NormalizeCode(code),
            name.Trim(),
            NormalizeCode(groupCode),
            groupName.Trim(),
            groupSortOrder,
            actionSortOrder,
            DateTime.UtcNow);
    }

    public static Permission Restore(
        Guid permissionId,
        string code,
        string name,
        string groupCode,
        string groupName,
        int groupSortOrder,
        int actionSortOrder,
        DateTime createdAt)
    {
        if (permissionId == Guid.Empty)
        {
            throw ValidationError("PERMISSION_ID_REQUIRED", "Permission id is required.");
        }

        Validate(code, name, groupCode, groupName, groupSortOrder, actionSortOrder);

        return new Permission(
            permissionId,
            NormalizeCode(code),
            name.Trim(),
            NormalizeCode(groupCode),
            groupName.Trim(),
            groupSortOrder,
            actionSortOrder,
            createdAt);
    }

    public void UpdateDisplay(
        string name,
        string groupName,
        int groupSortOrder,
        int actionSortOrder)
    {
        ValidateName(name, "PERMISSION_NAME_REQUIRED", "Permission name is required.");
        ValidateLength(name, MaxNameLength, "PERMISSION_NAME_TOO_LONG", "Permission name is too long.");
        ValidateName(groupName, "PERMISSION_GROUP_NAME_REQUIRED", "Permission group name is required.");
        ValidateLength(groupName, MaxGroupNameLength, "PERMISSION_GROUP_NAME_TOO_LONG", "Permission group name is too long.");
        ValidateSortOrder(groupSortOrder, "PERMISSION_GROUP_SORT_ORDER_INVALID", "Permission group sort order must be zero or greater.");
        ValidateSortOrder(actionSortOrder, "PERMISSION_ACTION_SORT_ORDER_INVALID", "Permission action sort order must be zero or greater.");

        Name = name.Trim();
        GroupName = groupName.Trim();
        GroupSortOrder = groupSortOrder;
        ActionSortOrder = actionSortOrder;
    }

    private static void Validate(
        string code,
        string name,
        string groupCode,
        string groupName,
        int groupSortOrder,
        int actionSortOrder)
    {
        ValidateCode(code, "PERMISSION_CODE_REQUIRED", "Permission code is required.");
        ValidateLength(code, MaxCodeLength, "PERMISSION_CODE_TOO_LONG", "Permission code is too long.");
        ValidateCode(groupCode, "PERMISSION_GROUP_CODE_REQUIRED", "Permission group code is required.");
        ValidateLength(groupCode, MaxGroupCodeLength, "PERMISSION_GROUP_CODE_TOO_LONG", "Permission group code is too long.");
        ValidateName(name, "PERMISSION_NAME_REQUIRED", "Permission name is required.");
        ValidateLength(name, MaxNameLength, "PERMISSION_NAME_TOO_LONG", "Permission name is too long.");
        ValidateName(groupName, "PERMISSION_GROUP_NAME_REQUIRED", "Permission group name is required.");
        ValidateLength(groupName, MaxGroupNameLength, "PERMISSION_GROUP_NAME_TOO_LONG", "Permission group name is too long.");
        ValidateSortOrder(groupSortOrder, "PERMISSION_GROUP_SORT_ORDER_INVALID", "Permission group sort order must be zero or greater.");
        ValidateSortOrder(actionSortOrder, "PERMISSION_ACTION_SORT_ORDER_INVALID", "Permission action sort order must be zero or greater.");

        var normalizedCode = NormalizeCode(code);
        var normalizedGroupCode = NormalizeCode(groupCode);

        if (!normalizedCode.StartsWith($"{normalizedGroupCode}.", StringComparison.Ordinal))
        {
            throw ValidationError(
                "PERMISSION_CODE_GROUP_MISMATCH",
                "Permission code must start with its group code.");
        }
    }

    private static void ValidateDefinedCode(string code)
    {
        var normalizedCode = NormalizeCode(code);

        if (!PermissionCodes.IsDefined(normalizedCode))
        {
            throw ValidationError(
                "PERMISSION_CODE_NOT_DEFINED",
                "Permission code must be defined in PermissionCodes.");
        }

        var expectedGroupCode = PermissionCodes.GetGroupCode(normalizedCode);

        if (!normalizedCode.StartsWith($"{expectedGroupCode}.", StringComparison.Ordinal))
        {
            throw ValidationError(
                "PERMISSION_CODE_GROUP_INVALID",
                "Permission code must include a valid group code.");
        }
    }

    private static void ValidateCode(string value, string errorCode, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationError(errorCode, message);
        }

        var normalizedValue = NormalizeCode(value);

        if (normalizedValue.Any(character =>
                !char.IsLower(character) &&
                !char.IsDigit(character) &&
                character != '_' &&
                character != '.'))
        {
            throw ValidationError(
                "PERMISSION_CODE_FORMAT_INVALID",
                "Permission code can only contain lowercase letters, numbers, underscore, and dot.");
        }
    }

    private static void ValidateName(string value, string errorCode, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationError(errorCode, message);
        }
    }

    private static void ValidateLength(string value, int maxLength, string errorCode, string message)
    {
        if (value.Trim().Length > maxLength)
        {
            throw ValidationError(errorCode, message);
        }
    }

    private static void ValidateSortOrder(int value, string errorCode, string message)
    {
        if (value < 0)
        {
            throw ValidationError(errorCode, message);
        }
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    private static DomainException ValidationError(string errorCode, string message)
    {
        return new DomainException(errorCode, message, DomainErrorType.Validation);
    }
}
