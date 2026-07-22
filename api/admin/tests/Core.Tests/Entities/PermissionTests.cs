using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using FluentAssertions;

namespace Core.Tests.Entities;

public sealed class PermissionTests
{
    public static TheoryData<string, string> NotificationPermissionCodes => new()
    {
        { PermissionCodes.NotificationEventTypes.View, "notification_event_type" },
        { PermissionCodes.NotificationTemplates.View, "notification_template" },
        { PermissionCodes.NotificationTemplates.Create, "notification_template" },
        { PermissionCodes.NotificationTemplates.Update, "notification_template" },
        { PermissionCodes.NotificationLogs.View, "notification_log" },
    };

    public static TheoryData<string> LegacyNotificationPermissionCodes => new()
    {
        "notification.view_event_types",
        "notification.view_templates",
        "notification.create_template",
        "notification.update_template",
        "notification.view_logs",
    };

    [Theory]
    [MemberData(nameof(NotificationPermissionCodes))]
    public void PermissionCode_TargetNotificationCode_IsDefinedAndHasExpectedGroup(
        string code,
        string expectedGroupCode)
    {
        PermissionCodes.All.Should().Contain(code);
        PermissionCodes.IsDefined(code).Should().BeTrue();
        PermissionCodes.GetGroupCode(code).Should().Be(expectedGroupCode);
    }

    [Theory]
    [MemberData(nameof(LegacyNotificationPermissionCodes))]
    public void PermissionCode_LegacyNotificationCode_IsNotDefined(string code)
    {
        PermissionCodes.IsDefined(code).Should().BeFalse();
        PermissionCodes.All.Should().NotContain(code);
    }

    [Theory]
    [MemberData(nameof(NotificationPermissionCodes))]
    public void Create_TargetNotificationCodeWithMatchingGroup_CreatesPermission(
        string code,
        string groupCode)
    {
        var permission = Permission.Create(
            code,
            "Notification permission",
            groupCode,
            "Notification",
            code[(code.IndexOf('.') + 1)..],
            "View",
            1,
            1);

        permission.Code.Should().Be(code);
        permission.GroupCode.Should().Be(groupCode);
        permission.ActionCode.Should().Be(code[(code.IndexOf('.') + 1)..]);
        permission.ActionName.Should().Be("View");
    }

    [Fact]
    public void Create_TargetNotificationCodeWithMismatchedGroup_ThrowsDomainException()
    {
        var action = () => Permission.Create(
            PermissionCodes.NotificationTemplates.View,
            "View Notification Template",
            "notification",
            "Notification",
            "view",
            "View",
            1,
            1);

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("PERMISSION_CODE_GROUP_MISMATCH");
    }

    [Fact]
    public void Create_ActionCodeDoesNotMatchFullCode_ThrowsDomainException()
    {
        var action = () => Permission.Create(
            PermissionCodes.Roles.View,
            "View Role",
            "role",
            "Role",
            "update",
            "Update",
            1,
            1);

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("PERMISSION_CODE_GROUP_MISMATCH");
    }
}
