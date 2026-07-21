using System.Reflection;
using Api.Controllers.Notifications;
using Core.Entities.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;

namespace Api.Tests.Controllers.Notifications;

public sealed class NotificationAuthorizationPolicyTests
{
    public static TheoryData<Type, string, string> ActionPolicies => new()
    {
        {
            typeof(NotificationEventTypesController),
            nameof(NotificationEventTypesController.GetList),
            PermissionCodes.NotificationEventTypes.View
        },
        {
            typeof(NotificationTemplatesController),
            nameof(NotificationTemplatesController.GetPaged),
            PermissionCodes.NotificationTemplates.View
        },
        {
            typeof(NotificationTemplatesController),
            nameof(NotificationTemplatesController.GetById),
            PermissionCodes.NotificationTemplates.View
        },
        {
            typeof(NotificationTemplatesController),
            nameof(NotificationTemplatesController.Create),
            PermissionCodes.NotificationTemplates.Create
        },
        {
            typeof(NotificationTemplatesController),
            nameof(NotificationTemplatesController.Update),
            PermissionCodes.NotificationTemplates.Update
        },
        {
            typeof(NotificationTemplatesController),
            nameof(NotificationTemplatesController.ToggleStatus),
            PermissionCodes.NotificationTemplates.Update
        },
        {
            typeof(NotificationLogsController),
            nameof(NotificationLogsController.GetPaged),
            PermissionCodes.NotificationLogs.View
        },
    };

    [Theory]
    [MemberData(nameof(ActionPolicies))]
    public void Action_UsesExpectedPermissionPolicy(
        Type controllerType,
        string actionName,
        string expectedPolicy)
    {
        var action = controllerType.GetMethod(actionName, BindingFlags.Instance | BindingFlags.Public);

        action.Should().NotBeNull();
        var authorizeAttribute = action!.GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Policy.Should().Be(expectedPolicy);
    }
}
