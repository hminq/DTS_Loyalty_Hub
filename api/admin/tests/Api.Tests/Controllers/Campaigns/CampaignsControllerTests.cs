using System.Reflection;
using System.Text.Json;
using Api.Authentication;
using Api.Controllers.Campaigns;
using Api.Dtos.Requests.Campaigns;
using Api.Dtos.Responses;
using Api.Dtos.Responses.Campaigns;
using Api.Localization;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Api.Tests.Controllers.Campaigns;

public sealed class CampaignsControllerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<ICurrentAdminContext> _adminContext = new();
    private readonly Mock<IValidator<GetCampaignsRequestDto>> _listValidator = new();
    private readonly Mock<IValidator<CampaignWriteRequestDto>> _campaignValidator = new();
    private readonly Mock<IValidator<CampaignActionWriteRequestDto>> _actionValidator = new();

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedCampaignWithLocation()
    {
        var actorUserId = Guid.NewGuid();
        var request = ValidCampaignRequest();
        var expected = CampaignResult();
        _adminContext.SetupGet(context => context.UserId).Returns(actorUserId);
        SetupValid(_campaignValidator);
        _sender.Setup(sender => sender.Send(
                It.IsAny<CreateCampaignCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var controller = CreateController();

        var actionResult = await controller.Create(request, CancellationToken.None);

        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Which;
        created.ActionName.Should().Be(nameof(CampaignsController.GetById));
        created.RouteValues.Should().ContainKey("campaignId")
            .WhoseValue.Should().Be(expected.CampaignId);
        var response = created.Value.Should()
            .BeOfType<ApiResponseDto<CampaignDetailResponseDto>>().Which;
        response.Data.CampaignId.Should().Be(expected.CampaignId);
        response.Data.Status.Should().Be("DRAFT");
        _sender.Verify(sender => sender.Send(
            It.Is<CreateCampaignCommand>(command =>
                command.ActorUserId == actorUserId &&
                command.CampaignName == request.CampaignName &&
                command.EventType == request.EventType &&
                command.ConditionJson.Contains("NORMAL")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsValidationErrorWithoutCommand()
    {
        SetupInvalid(
            _campaignValidator,
            new ValidationFailure("campaignName", "Campaign name is required.")
            {
                ErrorCode = "CAMPAIGN_NAME_REQUIRED"
            });
        var controller = CreateController();

        var actionResult = await controller.Create(
            new CampaignWriteRequestDto(),
            CancellationToken.None);

        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Which;
        var response = badRequest.Value.Should().BeOfType<ApiErrorResponseDto>().Which;
        response.Error.Code.Should().Be("VALIDATION_ERROR");
        response.Error.Details.Should().ContainSingle(detail =>
            detail.Field == "campaignName" &&
            detail.Code == "CAMPAIGN_NAME_REQUIRED");
        _sender.Verify(sender => sender.Send(
            It.IsAny<CreateCampaignCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _adminContext.VerifyGet(context => context.UserId, Times.Never);
    }

    [Fact]
    public async Task CreateAction_ValidRequest_ReturnsCreatedActionWithLocation()
    {
        var campaignId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        var request = ValidActionRequest();
        var expected = ActionResult();
        _adminContext.SetupGet(context => context.UserId).Returns(actorUserId);
        SetupValid(_actionValidator);
        _sender.Setup(sender => sender.Send(
                It.IsAny<CreateCampaignActionCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var controller = CreateController();

        var actionResult = await controller.CreateAction(
            campaignId,
            request,
            CancellationToken.None);

        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Which;
        created.ActionName.Should().Be(nameof(CampaignsController.GetActionById));
        created.RouteValues.Should().ContainKey("campaignId")
            .WhoseValue.Should().Be(campaignId);
        created.RouteValues.Should().ContainKey("actionId")
            .WhoseValue.Should().Be(expected.ActionId);
        var response = created.Value.Should()
            .BeOfType<ApiResponseDto<CampaignActionResponseDto>>().Which;
        response.Data.ActionId.Should().Be(expected.ActionId);
        response.Data.UsedCount.Should().Be(0);
        _sender.Verify(sender => sender.Send(
            It.Is<CreateCampaignActionCommand>(command =>
                command.CampaignId == campaignId &&
                command.ActorUserId == actorUserId &&
                command.ExecuteOrder == request.ExecuteOrder &&
                command.ActionConfigJson.Contains("FIXED_AMOUNT")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCampaign_ValidId_ReturnsNoContent()
    {
        var campaignId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        _adminContext.SetupGet(context => context.UserId).Returns(actorUserId);
        _sender.Setup(sender => sender.Send(
                It.IsAny<DeleteCampaignCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = CreateController();

        var result = await controller.Delete(campaignId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _sender.Verify(sender => sender.Send(
            It.Is<DeleteCampaignCommand>(command =>
                command.CampaignId == campaignId &&
                command.ActorUserId == actorUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAction_ValidIds_ReturnsNoContent()
    {
        var campaignId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var actorUserId = Guid.NewGuid();
        _adminContext.SetupGet(context => context.UserId).Returns(actorUserId);
        _sender.Setup(sender => sender.Send(
                It.IsAny<DeleteCampaignActionCommand>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = CreateController();

        var result = await controller.DeleteAction(
            campaignId,
            actionId,
            CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _sender.Verify(sender => sender.Send(
            It.Is<DeleteCampaignActionCommand>(command =>
                command.CampaignId == campaignId &&
                command.ActionId == actionId &&
                command.ActorUserId == actorUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(nameof(CampaignsController.Create), PermissionCodes.Campaigns.Create)]
    [InlineData(nameof(CampaignsController.Update), PermissionCodes.Campaigns.Update)]
    [InlineData(nameof(CampaignsController.Delete), PermissionCodes.Campaigns.Delete)]
    [InlineData(nameof(CampaignsController.GetActionById), PermissionCodes.Campaigns.View)]
    [InlineData(nameof(CampaignsController.CreateAction), PermissionCodes.Campaigns.Create)]
    [InlineData(nameof(CampaignsController.UpdateAction), PermissionCodes.Campaigns.Update)]
    [InlineData(nameof(CampaignsController.DeleteAction), PermissionCodes.Campaigns.Delete)]
    public void PhaseThreeEndpoints_UseExpectedPolicy(string methodName, string permission)
    {
        var method = typeof(CampaignsController).GetMethod(methodName);

        method.Should().NotBeNull();
        method!.GetCustomAttribute<AuthorizeAttribute>()!.Policy.Should().Be(permission);
    }

    private CampaignsController CreateController() => new(
        _sender.Object,
        _adminContext.Object,
        _listValidator.Object,
        _campaignValidator.Object,
        _actionValidator.Object,
        CreateValidationErrorMapper());

    private static ValidationErrorMapper CreateValidationErrorMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddScoped<ApiMessageResolver>();
        services.AddScoped<ValidationErrorMapper>();
        return services.BuildServiceProvider().GetRequiredService<ValidationErrorMapper>();
    }

    private static void SetupValid<T>(Mock<IValidator<T>> validator)
        where T : class
    {
        validator.Setup(item => item.ValidateAsync(
                It.IsAny<T>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static void SetupInvalid<T>(
        Mock<IValidator<T>> validator,
        params ValidationFailure[] failures)
        where T : class
    {
        validator.Setup(item => item.ValidateAsync(
                It.IsAny<T>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));
    }

    private static CampaignWriteRequestDto ValidCampaignRequest() => new()
    {
        CampaignName = "Normal registration reward",
        Description = "Issue points after normal registration.",
        EventType = "CUSTOMER_ACCOUNT_REGISTERED",
        Condition = Json("""{"sources":["NORMAL"]}"""),
        StartDate = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
        EndDate = new DateTimeOffset(2026, 8, 31, 23, 59, 59, TimeSpan.Zero),
        ScheduleCron = "0 0 2 * * ?",
        DurationHour = 2,
        UserLimitTotal = 1,
        UserLimitSession = 1
    };

    private static CampaignActionWriteRequestDto ValidActionRequest() => new()
    {
        ActionType = "ISSUE_POINT",
        ActionConfig = Json("""
            {"calculationType":"FIXED_AMOUNT","recipient":"EVENT_CUSTOMER","amount":50,
             "calculationBase":null,"percentage":null,"maximumPoints":null}
            """),
        ExecuteOrder = 1
    };

    private static CampaignDetailResult CampaignResult()
    {
        var now = new DateTime(2026, 7, 27, 10, 0, 0, DateTimeKind.Utc);
        return new CampaignDetailResult(
            Guid.NewGuid(),
            "Normal registration reward",
            null,
            null,
            "CUSTOMER_ACCOUNT_REGISTERED",
            now.AddDays(1),
            now.AddDays(31),
            """{"sources":["NORMAL"]}""",
            "0 0 2 * * ?",
            2,
            1,
            1,
            "DRAFT",
            now,
            now,
            [],
            [],
            0);
    }

    private static CampaignActionResult ActionResult()
    {
        return new CampaignActionResult(
            Guid.NewGuid(),
            "ISSUE_POINT",
            """
            {"calculationType":"FIXED_AMOUNT","recipient":"EVENT_CUSTOMER","amount":50,
             "calculationBase":null,"percentage":null,"maximumPoints":null}
            """,
            1,
            null,
            null,
            0,
            null,
            null,
            0,
            new DateTime(2026, 7, 27, 10, 0, 0, DateTimeKind.Utc));
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
