using System.Reflection;
using Api.Authentication;
using Api.Controllers.CustomerUsers;
using Api.Dtos.Requests.CustomerUsers;
using Api.Dtos.Responses;
using Api.Dtos.Responses.CustomerUsers;
using Api.Localization;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Common;
using Core.UseCases.CustomerUsers.Commands;
using Core.UseCases.CustomerUsers.Queries;
using Core.UseCases.CustomerUsers.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Api.Tests.Controllers.CustomerUsers;

public sealed class CustomerUsersControllerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<ICurrentAdminContext> _currentAdminContext = new();
    private readonly Mock<IValidator<GetCustomerUsersRequestDto>> _validator = new();
    private readonly Mock<IValidator<GetCustomerUserHistoryRequestDto>> _historyValidator = new();
    private readonly Mock<IValidator<UpdateCustomerUserRequestDto>> _updateValidator = new();
    private readonly Mock<IValidator<UpdateCustomerUserStatusRequestDto>> _statusValidator = new();

    [Fact]
    public async Task GetList_ValidRequest_ReturnsPagedCustomerResponse()
    {
        var tierId = Guid.NewGuid();
        var request = new GetCustomerUsersRequestDto
        {
            Page = 2,
            PageSize = 10,
            Keyword = "minh",
            Status = "ENABLE",
            TierId = tierId
        };
        var item = new CustomerUserListItemResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "minh",
            "minh@example.com",
            "Minh Nguyen",
            null,
            tierId,
            "Gold",
            "ENABLE",
            DateTime.UtcNow);
        SetupValid();
        _sender.Setup(sender => sender.Send(
                It.IsAny<GetCustomerUsersQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CustomerUserListItemResult>([item], 2, 10, 21));
        var controller = CreateController();

        var actionResult = await controller.GetList(request, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should()
            .BeOfType<ApiResponseDto<IReadOnlyCollection<CustomerUserListItemResponseDto>>>()
            .Which;
        response.Data.Should().ContainSingle().Which.CustomerId.Should().Be(item.CustomerId);
        response.Meta.Should().BeEquivalentTo(new
        {
            Page = 2,
            PageSize = 10,
            TotalItems = 21,
            TotalPages = 3
        });
        _sender.Verify(sender => sender.Send(
            It.Is<GetCustomerUsersQuery>(query =>
                query.Page == request.Page &&
                query.PageSize == request.PageSize &&
                query.Keyword == request.Keyword &&
                query.Status == request.Status &&
                query.TierId == request.TierId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetList_InvalidRequest_ReturnsValidationWrapperWithoutSendingQuery()
    {
        _validator.Setup(validator => validator.ValidateAsync(
                It.IsAny<GetCustomerUsersRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("tierId", string.Empty) { ErrorCode = "TIER_ID_INVALID" }
            ]));
        var controller = CreateController();

        var actionResult = await controller.GetList(
            new GetCustomerUsersRequestDto { TierId = Guid.Empty },
            CancellationToken.None);

        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Which;
        var response = badRequest.Value.Should().BeOfType<ApiErrorResponseDto>().Which;
        response.Error.Code.Should().Be("VALIDATION_ERROR");
        response.Error.Details.Should().ContainSingle(detail =>
            detail.Field == "tierId" && detail.Code == "TIER_ID_INVALID");
        _sender.Verify(sender => sender.Send(
            It.IsAny<GetCustomerUsersQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetById_ReturnsProfileAndTierProgress()
    {
        var customerId = Guid.NewGuid();
        var tier = new CustomerUserTierResult(Guid.NewGuid(), "Gold", 1000, 3, 2);
        var expected = new CustomerUserDetailResult(
            customerId,
            Guid.NewGuid(),
            "customer",
            "customer@example.com",
            "Customer User",
            null,
            "ENABLE",
            DateTime.UtcNow,
            650,
            350,
            tier);
        _sender.Setup(sender => sender.Send(
                It.Is<GetCustomerUserByIdQuery>(query => query.CustomerId == customerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var controller = CreateController();

        var actionResult = await controller.GetById(customerId, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should().BeOfType<ApiResponseDto<CustomerUserResponseDto>>().Which;
        response.Data.CustomerId.Should().Be(customerId);
        response.Data.CurrentTierPoint.Should().Be(650);
        response.Data.NextTierPoint.Should().Be(350);
        response.Data.Tier.Should().NotBeNull();
        response.Data.Tier!.TierId.Should().Be(tier.TierId);
    }

    [Fact]
    public async Task GetPoints_ReturnsTierProgressAndPointWallet()
    {
        var customerId = Guid.NewGuid();
        var tier = new CustomerUserTierResult(Guid.NewGuid(), "Gold", 1000, 3, 2);
        var updatedAt = DateTime.UtcNow;
        var expected = new CustomerUserPointsResult(
            customerId,
            650,
            350,
            tier,
            400,
            50,
            1200,
            600,
            150,
            updatedAt);
        _sender.Setup(sender => sender.Send(
                It.Is<GetCustomerUserPointsQuery>(query => query.CustomerId == customerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var controller = CreateController();

        var actionResult = await controller.GetPoints(customerId, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should()
            .BeOfType<ApiResponseDto<CustomerUserPointsResponseDto>>()
            .Which;
        response.Data.CustomerId.Should().Be(customerId);
        response.Data.ActivePoint.Should().Be(400);
        response.Data.LockedPoint.Should().Be(50);
        response.Data.LifetimePoint.Should().Be(1200);
        response.Data.SpentPoint.Should().Be(600);
        response.Data.ExpiredPoint.Should().Be(150);
        response.Data.UpdatedAt.Should().Be(updatedAt);
        response.Data.Tier.Should().NotBeNull();
        response.Data.Tier!.TierId.Should().Be(tier.TierId);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsUpdatedCustomerAndUsesActor()
    {
        var customerId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var request = new UpdateCustomerUserRequestDto
        {
            Email = "updated@example.com",
            FullName = "Updated Customer",
            PhoneNumber = "0123456789"
        };
        var expected = new CustomerUserDetailResult(
            customerId,
            Guid.NewGuid(),
            "customer",
            request.Email,
            request.FullName,
            request.PhoneNumber,
            "ENABLE",
            DateTime.UtcNow,
            10,
            20,
            null);
        _currentAdminContext.SetupGet(context => context.UserId).Returns(actorUserId);
        _updateValidator.Setup(validator => validator.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _sender.Setup(sender => sender.Send(
                It.Is<UpdateCustomerUserCommand>(command =>
                    command.CustomerId == customerId &&
                    command.Email == request.Email &&
                    command.FullName == request.FullName &&
                    command.PhoneNumber == request.PhoneNumber &&
                    command.ActorUserId == actorUserId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var controller = CreateController();

        var actionResult = await controller.Update(customerId, request, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should().BeOfType<ApiResponseDto<CustomerUserResponseDto>>().Which;
        response.Data.CustomerId.Should().Be(customerId);
        response.Data.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Update_InvalidRequest_ReturnsValidationWrapperWithoutSendingCommand()
    {
        var request = new UpdateCustomerUserRequestDto { Email = "invalid" };
        _updateValidator.Setup(validator => validator.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("email", string.Empty) { ErrorCode = "EMAIL_INVALID" }
            ]));
        var controller = CreateController();

        var actionResult = await controller.Update(Guid.NewGuid(), request, CancellationToken.None);

        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Which;
        var response = badRequest.Value.Should().BeOfType<ApiErrorResponseDto>().Which;
        response.Error.Details.Should().ContainSingle(detail =>
            detail.Field == "email" && detail.Code == "EMAIL_INVALID");
        _sender.Verify(sender => sender.Send(
            It.IsAny<UpdateCustomerUserCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_ValidRequest_ReturnsNoContentAndUsesActor()
    {
        var customerId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var request = new UpdateCustomerUserStatusRequestDto { Status = "DISABLE" };
        _currentAdminContext.SetupGet(context => context.UserId).Returns(actorUserId);
        _statusValidator.Setup(validator => validator.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _sender.Setup(sender => sender.Send(
                It.Is<UpdateCustomerUserStatusCommand>(command =>
                    command.CustomerId == customerId &&
                    command.Status == "DISABLE" &&
                    command.ActorUserId == actorUserId),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = CreateController();

        var result = await controller.UpdateStatus(customerId, request, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ReadEndpoints_UseCustomerViewPolicy()
    {
        var methods = new[]
        {
            nameof(CustomerUsersController.GetList),
            nameof(CustomerUsersController.GetById),
            nameof(CustomerUsersController.GetPoints)
        };

        foreach (var methodName in methods)
        {
            var method = typeof(CustomerUsersController).GetMethod(methodName);

            method.Should().NotBeNull();
            method!.GetCustomAttribute<AuthorizeAttribute>()!.Policy
                .Should().Be(PermissionCodes.CustomerUsers.View);
        }
    }

    [Theory]
    [InlineData(nameof(CustomerUsersController.Update), PermissionCodes.CustomerUsers.Update)]
    [InlineData(nameof(CustomerUsersController.UpdateStatus), PermissionCodes.CustomerUsers.Disable)]
    public void WriteEndpoints_UseExpectedPolicy(string methodName, string permission)
    {
        var method = typeof(CustomerUsersController).GetMethod(methodName);

        method.Should().NotBeNull();
        method!.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be(permission);
    }

    [Fact]
    public void Controller_UsesExpectedRoute()
    {
        var route = typeof(CustomerUsersController).GetCustomAttribute<RouteAttribute>();

        route.Should().NotBeNull();
        route!.Template.Should().Be("api/admin/customer-users");
    }

    private CustomerUsersController CreateController() => new(
        _sender.Object,
        _currentAdminContext.Object,
        _validator.Object,
        _historyValidator.Object,
        _updateValidator.Object,
        _statusValidator.Object,
        CreateValidationErrorMapper());

    private void SetupValid()
    {
        _validator.Setup(validator => validator.ValidateAsync(
                It.IsAny<GetCustomerUsersRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static ValidationErrorMapper CreateValidationErrorMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddSingleton<ApiMessageResolver>();
        services.AddScoped<ValidationErrorMapper>();
        return services.BuildServiceProvider().GetRequiredService<ValidationErrorMapper>();
    }
}
