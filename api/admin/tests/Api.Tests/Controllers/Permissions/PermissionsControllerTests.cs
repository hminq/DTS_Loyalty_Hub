using Api.Controllers.Permissions;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Permissions;
using Core.UseCases.Permissions.Queries;
using Core.UseCases.Permissions.Results;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Api.Tests.Controllers.Permissions;

public sealed class PermissionsControllerTests
{
    [Fact]
    public async Task Get_ReturnsExplicitActionMetadata()
    {
        var sender = new Mock<ISender>();
        var permission = new PermissionResult(
            Guid.NewGuid(),
            "admin_user.reset_password",
            "Reset Admin User Password",
            "reset_password",
            "Reset Password",
            60);
        sender.Setup(instance => instance.Send(
                It.IsAny<GetPermissionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new PermissionGroupResult("admin_user", "Admin User", 30, [permission])
            ]);
        var controller = new PermissionsController(sender.Object);

        var actionResult = await controller.Get(CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should()
            .BeOfType<ApiResponseDto<IReadOnlyCollection<PermissionGroupResponseDto>>>()
            .Which;
        response.Data.Should().ContainSingle()
            .Which.Permissions.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                permission.PermissionId,
                permission.Code,
                permission.Name,
                permission.ActionCode,
                permission.ActionName,
                permission.ActionSortOrder
            });
    }
}
