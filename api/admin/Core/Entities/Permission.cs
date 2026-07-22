using Core.Entities.Constants;
using Core.Exceptions;

namespace Core.Entities;

public class Permission
{
    private const int MaxCodeLength = 100;
    private const int MaxNameLength = 100;
    private const int MaxGroupCodeLength = 50;
    private const int MaxGroupNameLength = 100;
    private const int MaxActionCodeLength = 50;
    private const int MaxActionNameLength = 100;

    private Permission(
        Guid permissionId,
        string code,
        string name,
        string groupCode,
        string groupName,
        string actionCode,
        string actionName,
        int groupSortOrder,
        int actionSortOrder,
        DateTime createdAt)
    {
        PermissionId = permissionId;
        Code = code;
        Name = name;
        GroupCode = groupCode;
        GroupName = groupName;
        ActionCode = actionCode;
        ActionName = actionName;
        GroupSortOrder = groupSortOrder;
        ActionSortOrder = actionSortOrder;
        CreatedAt = createdAt;
    }

    public Guid PermissionId { get; private set; }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string GroupCode { get; private set; }

    public string GroupName { get; private set; }

    public string ActionCode { get; private set; }

    public string ActionName { get; private set; }

    public int GroupSortOrder { get; private set; }

    public int ActionSortOrder { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Permission Create(
        string code,
        string name,
        string groupCode,
        string groupName,
        string actionCode,
        string actionName,
        int groupSortOrder,
        int actionSortOrder)
    {
        Validate(code, name, groupCode, groupName, actionCode, actionName, groupSortOrder, actionSortOrder);
        ValidateDefinedCode(code);

        return new Permission(
            Guid.NewGuid(),
            NormalizeCode(code),
            name.Trim(),
            NormalizeCode(groupCode),
            groupName.Trim(),
            NormalizeCode(actionCode),
            actionName.Trim(),
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
        string actionCode,
        string actionName,
        int groupSortOrder,
        int actionSortOrder,
        DateTime createdAt)
    {
        if (permissionId == Guid.Empty)
        {
            throw ValidationError("PERMISSION_ID_REQUIRED");
        }

        Validate(code, name, groupCode, groupName, actionCode, actionName, groupSortOrder, actionSortOrder);

        return new Permission(
            permissionId,
            NormalizeCode(code),
            name.Trim(),
            NormalizeCode(groupCode),
            groupName.Trim(),
            NormalizeCode(actionCode),
            actionName.Trim(),
            groupSortOrder,
            actionSortOrder,
            createdAt);
    }

    public void UpdateDisplay(
        string name,
        string groupName,
        string actionName,
        int groupSortOrder,
        int actionSortOrder)
    {
        ValidateName(name, "PERMISSION_NAME_REQUIRED");
        ValidateLength(name, MaxNameLength, "PERMISSION_NAME_TOO_LONG");
        ValidateName(groupName, "PERMISSION_GROUP_NAME_REQUIRED");
        ValidateLength(groupName, MaxGroupNameLength, "PERMISSION_GROUP_NAME_TOO_LONG");
        ValidateName(actionName, "PERMISSION_ACTION_NAME_REQUIRED");
        ValidateLength(actionName, MaxActionNameLength, "PERMISSION_ACTION_NAME_TOO_LONG");
        ValidateSortOrder(groupSortOrder, "PERMISSION_GROUP_SORT_ORDER_INVALID");
        ValidateSortOrder(actionSortOrder, "PERMISSION_ACTION_SORT_ORDER_INVALID");

        Name = name.Trim();
        GroupName = groupName.Trim();
        ActionName = actionName.Trim();
        GroupSortOrder = groupSortOrder;
        ActionSortOrder = actionSortOrder;
    }

    private static void Validate(
        string code,
        string name,
        string groupCode,
        string groupName,
        string actionCode,
        string actionName,
        int groupSortOrder,
        int actionSortOrder)
    {
        ValidateCode(code, "PERMISSION_CODE_REQUIRED");
        ValidateLength(code, MaxCodeLength, "PERMISSION_CODE_TOO_LONG");
        ValidateCode(groupCode, "PERMISSION_GROUP_CODE_REQUIRED");
        ValidateLength(groupCode, MaxGroupCodeLength, "PERMISSION_GROUP_CODE_TOO_LONG");
        ValidateActionCode(actionCode);
        ValidateLength(actionCode, MaxActionCodeLength, "PERMISSION_ACTION_CODE_TOO_LONG");
        ValidateName(name, "PERMISSION_NAME_REQUIRED");
        ValidateLength(name, MaxNameLength, "PERMISSION_NAME_TOO_LONG");
        ValidateName(groupName, "PERMISSION_GROUP_NAME_REQUIRED");
        ValidateLength(groupName, MaxGroupNameLength, "PERMISSION_GROUP_NAME_TOO_LONG");
        ValidateName(actionName, "PERMISSION_ACTION_NAME_REQUIRED");
        ValidateLength(actionName, MaxActionNameLength, "PERMISSION_ACTION_NAME_TOO_LONG");
        ValidateSortOrder(groupSortOrder, "PERMISSION_GROUP_SORT_ORDER_INVALID");
        ValidateSortOrder(actionSortOrder, "PERMISSION_ACTION_SORT_ORDER_INVALID");

        var normalizedCode = NormalizeCode(code);
        var normalizedGroupCode = NormalizeCode(groupCode);
        var normalizedActionCode = NormalizeCode(actionCode);

        if (!string.Equals(
                normalizedCode,
                $"{normalizedGroupCode}.{normalizedActionCode}",
                StringComparison.Ordinal))
        {
            throw ValidationError("PERMISSION_CODE_GROUP_MISMATCH");
        }
    }

    private static void ValidateDefinedCode(string code)
    {
        var normalizedCode = NormalizeCode(code);

        if (!PermissionCodes.IsDefined(normalizedCode))
        {
            throw ValidationError("PERMISSION_CODE_NOT_DEFINED");
        }

        var expectedGroupCode = PermissionCodes.GetGroupCode(normalizedCode);

        if (!normalizedCode.StartsWith($"{expectedGroupCode}.", StringComparison.Ordinal))
        {
            throw ValidationError("PERMISSION_CODE_GROUP_INVALID");
        }
    }

    private static void ValidateCode(string value, string errorCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationError(errorCode);
        }

        var normalizedValue = NormalizeCode(value);

        if (normalizedValue.Any(character =>
                !char.IsLower(character) &&
                !char.IsDigit(character) &&
                character != '_' &&
                character != '.'))
        {
            throw ValidationError("PERMISSION_CODE_FORMAT_INVALID");
        }
    }

    private static void ValidateName(string value, string errorCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationError(errorCode);
        }
    }

    private static void ValidateActionCode(string value)
    {
        ValidateCode(value, "PERMISSION_ACTION_CODE_REQUIRED");

        if (NormalizeCode(value).Contains('.', StringComparison.Ordinal))
        {
            throw ValidationError("PERMISSION_ACTION_CODE_FORMAT_INVALID");
        }
    }

    private static void ValidateLength(string value, int maxLength, string errorCode)
    {
        if (value.Trim().Length > maxLength)
        {
            throw ValidationError(errorCode);
        }
    }

    private static void ValidateSortOrder(int value, string errorCode)
    {
        if (value < 0)
        {
            throw ValidationError(errorCode);
        }
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    private static DomainException ValidationError(string errorCode)
    {
        return new DomainException(errorCode, DomainErrorType.Validation);
    }
}
