using Api.Authentication;
using Api.Controllers.Roles;
using Api.Dtos.Requests.Roles;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Roles;
using Api.Localization;
using Api.Mappers;
using Core.UseCases.Common;
using Core.UseCases.Roles.Queries;
using Core.UseCases.Roles.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Api.Tests.Controllers.Roles;

public sealed class RolesControllerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<ICurrentAdminContext> _adminContext = new();
    private readonly Mock<IValidator<GetRolesRequestDto>> _getValidator = new();
    private readonly Mock<IValidator<CreateRoleRequestDto>> _createValidator = new();
    private readonly Mock<IValidator<UpdateRoleRequestDto>> _updateValidator = new();

    [Fact]
    public async Task GetById_ExistingRole_ReturnsDetailedPermissionMetadata()
    {
        var roleId = Guid.NewGuid();
        var permission = new RolePermissionDetailResult(
            Guid.NewGuid(),
            "notification_template.update",
            "Update Notification Template",
            "notification_template",
            "Notification Template",
            91,
            30);
        var result = new RoleDetailResult(
            roleId,
            "Notification Manager",
            [permission],
            DateTime.UtcNow);
        _sender.Setup(sender => sender.Send(
                It.Is<GetRoleByIdQuery>(query => query.RoleId == roleId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        var controller = CreateController();

        var actionResult = await controller.GetById(roleId, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should()
            .BeOfType<ApiResponseDto<RoleDetailResponseDto>>().Which;
        response.Data.RoleId.Should().Be(roleId);
        response.Data.PermissionIds.Should().Equal(permission.PermissionId);
        response.Data.Permissions.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            permission.PermissionId,
            permission.Code,
            permission.Name,
            permission.GroupCode,
            permission.GroupName,
            permission.GroupSortOrder,
            permission.ActionSortOrder
        });
    }

    [Fact]
    public async Task GetOptions_ValidRequest_ReturnsMinimalPagedRoleOptions()
    {
        var request = new GetRolesRequestDto
        {
            Page = 2,
            PageSize = 10,
            Keyword = "admin"
        };
        var option = new RoleOptionResult(Guid.NewGuid(), "System Admin");
        SetupValid(_getValidator);
        _sender.Setup(sender => sender.Send(
                It.IsAny<GetRoleOptionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<RoleOptionResult>([option], 2, 10, 11));
        var controller = CreateController();

        var actionResult = await controller.GetOptions(request, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should()
            .BeOfType<ApiResponseDto<IReadOnlyCollection<RoleOptionResponseDto>>>().Which;
        response.Data.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            option.RoleId,
            option.Name
        });
        response.Meta.Should().NotBeNull();
        response.Meta!.Page.Should().Be(2);
        response.Meta.TotalItems.Should().Be(11);
        response.Meta.TotalPages.Should().Be(2);
        _sender.Verify(sender => sender.Send(
            It.Is<GetRoleOptionsQuery>(query =>
                query.Page == request.Page &&
                query.PageSize == request.PageSize &&
                query.Keyword == request.Keyword),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOptions_InvalidRequest_ReturnsValidationWrapperWithoutSendingQuery()
    {
        _getValidator.Setup(validator => validator.ValidateAsync(
                It.IsAny<GetRolesRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("page", string.Empty) { ErrorCode = "PAGE_INVALID" }
            ]));
        var controller = CreateController();

        var actionResult = await controller.GetOptions(
            new GetRolesRequestDto { Page = 0 },
            CancellationToken.None);

        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Which;
        var response = badRequest.Value.Should().BeOfType<ApiErrorResponseDto>().Which;
        response.Error.Code.Should().Be("VALIDATION_ERROR");
        response.Error.Details.Should().ContainSingle(detail =>
            detail.Field == "page" && detail.Code == "PAGE_INVALID");
        _sender.Verify(sender => sender.Send(
            It.IsAny<GetRoleOptionsQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private RolesController CreateController() => new(
        _sender.Object,
        _adminContext.Object,
        _getValidator.Object,
        _createValidator.Object,
        _updateValidator.Object,
        CreateValidationErrorMapper());

    private static ValidationErrorMapper CreateValidationErrorMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddSingleton<ApiMessageResolver>();
        services.AddScoped<ValidationErrorMapper>();
        return services.BuildServiceProvider().GetRequiredService<ValidationErrorMapper>();
    }

    private static void SetupValid<T>(Mock<IValidator<T>> validator)
        where T : class
    {
        validator.Setup(instance => instance.ValidateAsync(
                It.IsAny<T>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }
}
