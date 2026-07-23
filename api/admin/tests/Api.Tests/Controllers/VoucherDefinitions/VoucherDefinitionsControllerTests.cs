using Api.Authentication;
using Api.Controllers.VoucherDefinitions;
using Api.Dtos.Requests.VoucherDefinitions;
using Api.Dtos.Responses;
using Api.Dtos.Responses.VoucherDefinitions;
using Api.Localization;
using Api.Mappers;
using Core.Entities.Constants;
using Core.UseCases.Common;
using Core.UseCases.VoucherDefinitions.Commands;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Api.Tests.Controllers.VoucherDefinitions;

public sealed class VoucherDefinitionsControllerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<ICurrentAdminContext> _adminContext = new();
    private readonly Mock<IValidator<GetVoucherDefinitionsRequestDto>> _getValidator = new();
    private readonly Mock<IValidator<CreateVoucherDefinitionRequestDto>> _createValidator = new();

    [Fact]
    public async Task Create_ValidRequest_SendsCommandWithActorAndReturnsCreatedAtAction()
    {
        var actorUserId = Guid.NewGuid();
        var request = ValidCreateRequest();
        var expected = Result(name: request.Name);
        _adminContext.SetupGet(x => x.UserId).Returns(actorUserId);
        SetupValid(_createValidator);
        _sender.Setup(x => x.Send(
                It.IsAny<CreateVoucherDefinitionCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var controller = CreateController();

        var actionResult = await controller.Create(request, CancellationToken.None);

        var created = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Which;
        created.ActionName.Should().Be(nameof(VoucherDefinitionsController.GetById));
        created.RouteValues.Should().ContainKey("voucherDefinitionId")
            .WhoseValue.Should().Be(expected.VoucherDefinitionId);
        var response = created.Value.Should()
            .BeOfType<ApiResponseDto<VoucherDefinitionResponseDto>>().Which;
        response.Data.VoucherDefinitionId.Should().Be(expected.VoucherDefinitionId);
        response.Data.Name.Should().Be(expected.Name);

        _sender.Verify(x => x.Send(
            It.Is<CreateVoucherDefinitionCommand>(command =>
                command.ActorUserId == actorUserId &&
                command.Code == request.Code &&
                command.Name == request.Name &&
                command.TotalStock == request.TotalStock),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsValidationErrorWithoutSendingCommand()
    {
        SetupInvalid(
            _createValidator,
            new ValidationFailure("name", "Voucher definition name is required.")
            {
                ErrorCode = "VOUCHER_DEFINITION_NAME_REQUIRED"
            });
        var controller = CreateController();

        var actionResult = await controller.Create(
            new CreateVoucherDefinitionRequestDto(),
            CancellationToken.None);

        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Which;
        var response = badRequest.Value.Should().BeOfType<ApiErrorResponseDto>().Which;
        response.Error.Code.Should().Be("VALIDATION_ERROR");
        response.Error.Details.Should().ContainSingle(detail =>
            detail.Field == "name" &&
            detail.Code == "VOUCHER_DEFINITION_NAME_REQUIRED");
        _sender.Verify(x => x.Send(
            It.IsAny<CreateVoucherDefinitionCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _adminContext.VerifyGet(x => x.UserId, Times.Never);
    }

    [Fact]
    public async Task GetList_ValidRequest_SendsQueryAndReturnsPagedResponse()
    {
        var request = new GetVoucherDefinitionsRequestDto
        {
            Page = 2,
            PageSize = 10,
            Keyword = "welcome"
        };
        var item = ListItemResult();
        SetupValid(_getValidator);
        _sender.Setup(x => x.Send(
                It.IsAny<GetVoucherDefinitionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<VoucherDefinitionListItemResult>([item], 2, 10, 21));
        var controller = CreateController();

        var actionResult = await controller.GetList(request, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should()
            .BeOfType<ApiResponseDto<IReadOnlyCollection<VoucherDefinitionListItemResponseDto>>>().Which;
        response.Data.Should().ContainSingle().Which.VoucherDefinitionId
            .Should().Be(item.VoucherDefinitionId);
        response.Meta.Should().NotBeNull();
        response.Meta!.Page.Should().Be(2);
        response.Meta.PageSize.Should().Be(10);
        response.Meta.TotalItems.Should().Be(21);
        response.Meta.TotalPages.Should().Be(3);
        _sender.Verify(x => x.Send(
            It.Is<GetVoucherDefinitionsQuery>(query =>
                query.Page == request.Page &&
                query.PageSize == request.PageSize &&
                query.Keyword == request.Keyword),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetList_InvalidRequest_ReturnsBadRequestWithoutSendingQuery()
    {
        SetupInvalid(
            _getValidator,
            new ValidationFailure("page", "Page must be greater than or equal to 1.")
            {
                ErrorCode = "PAGE_INVALID"
            });
        var controller = CreateController();

        var actionResult = await controller.GetList(
            new GetVoucherDefinitionsRequestDto { Page = 0 },
            CancellationToken.None);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        _sender.Verify(x => x.Send(
            It.IsAny<GetVoucherDefinitionsQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetById_SendsRouteIdAndReturnsWrappedResponse()
    {
        var voucherDefinitionId = Guid.NewGuid();
        var expected = Result(voucherDefinitionId);
        _sender.Setup(x => x.Send(
                It.IsAny<GetVoucherDefinitionByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var controller = CreateController();

        var actionResult = await controller.GetById(
            voucherDefinitionId,
            CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value.Should()
            .BeOfType<ApiResponseDto<VoucherDefinitionResponseDto>>().Which;
        response.Data.VoucherDefinitionId.Should().Be(voucherDefinitionId);
        _sender.Verify(x => x.Send(
            It.Is<GetVoucherDefinitionByIdQuery>(query =>
                query.VoucherDefinitionId == voucherDefinitionId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private VoucherDefinitionsController CreateController()
    {
        var localizerMock = new Mock<Microsoft.Extensions.Localization.IStringLocalizer<VoucherDefinitionOptions>>();
        var labelResolver = new Api.Localization.VoucherDefinitionOptionLabelResolver(localizerMock.Object);

        return new(
            _sender.Object,
            _adminContext.Object,
            _getValidator.Object,
            _createValidator.Object,
            CreateValidationErrorMapper(),
            labelResolver);
    }

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
        validator.Setup(x => x.ValidateAsync(
                It.IsAny<T>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static void SetupInvalid<T>(
        Mock<IValidator<T>> validator,
        params ValidationFailure[] failures)
        where T : class
    {
        validator.Setup(x => x.ValidateAsync(
                It.IsAny<T>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));
    }

    private static CreateVoucherDefinitionRequestDto ValidCreateRequest() => new()
    {
        Code = "WELCOME10",
        Name = "Welcome voucher",
        RewardType = VoucherRewardTypes.Fixed,
        RewardValue = 10,
        ValidityType = VoucherValidityTypes.Fixed,
        ValidFrom = new DateTime(2026, 1, 1),
        ValidTo = new DateTime(2026, 12, 31),
        GenerationType = VoucherGenerationTypes.None,
        PublishType = VoucherPublishTypes.Public,
        TotalStock = 100
    };

    private static VoucherDefinitionListItemResult ListItemResult(
        Guid? voucherDefinitionId = null,
        string name = "Welcome voucher") => new(
        voucherDefinitionId ?? Guid.NewGuid(),
        "WELCOME10",
        name,
        VoucherRewardTypes.Fixed,
        10,
        VoucherPublishTypes.Public,
        100,
        100,
        new DateTime(2026, 1, 1),
        null);

    private static VoucherDefinitionResult Result(
        Guid? voucherDefinitionId = null,
        string name = "Welcome voucher") => new(
        voucherDefinitionId ?? Guid.NewGuid(),
        "WELCOME10",
        name,
        null,
        null,
        VoucherRewardTypes.Fixed,
        10,
        VoucherValidityTypes.Fixed,
        new DateTime(2026, 1, 1),
        new DateTime(2026, 12, 31),
        null,
        VoucherGenerationTypes.None,
        VoucherPublishTypes.Public,
        100,
        100,
        new DateTime(2026, 1, 1),
        null);
}
