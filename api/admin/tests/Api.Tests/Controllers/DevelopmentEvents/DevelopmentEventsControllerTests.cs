using System.Reflection;
using System.Text.Json;
using Api.Controllers.DevelopmentEvents;
using Api.Dtos.Requests.DevelopmentEvents;
using Api.Dtos.Responses;
using Api.Dtos.Responses.DevelopmentEvents;
using Api.Localization;
using Api.Mappers;
using Api.Validators.DevelopmentEvents;
using Core.Abstractions;
using Core.Entities.Events;
using FluentAssertions;
using Messaging.Contracts.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Api.Tests.Controllers.DevelopmentEvents;

public sealed class DevelopmentEventsControllerTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 7, 31, 8, 30, 0, TimeSpan.Zero);

    private readonly Mock<IEventPublisher> _publisher = new();

    [Fact]
    public async Task PublishCustomerMissionCompleted_ValidRequest_PublishesExpectedEnvelope()
    {
        OutgoingEvent? publishedMessage = null;
        _publisher
            .Setup(publisher => publisher.PublishAsync(
                It.IsAny<OutgoingEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<OutgoingEvent, CancellationToken>((message, _) => publishedMessage = message)
            .Returns(Task.CompletedTask);
        var controller = CreateController(Environments.Development);
        var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var result = await controller.PublishCustomerMissionCompleted(
            new CustomerMissionCompletedRequestDto
            {
                CustomerId = customerId,
                MissionCode = "DAILY_LOGIN_7_DAYS",
                MissionCategory = "ENGAGEMENT",
                IsFirstCompletion = true,
                CompletionCount = 1
            },
            CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = ok.Value
            .Should().BeOfType<ApiResponseDto<MockEventPublishedResponseDto>>().Which;
        response.Data.EventId.Should().NotBeEmpty();
        response.Data.EventType.Should().Be(EventTypeCodes.CustomerMissionCompleted);
        response.Data.EventVersion.Should().Be(1);
        response.Data.OccurredAt.Should().Be(FixedNow.UtcDateTime);
        response.Data.RoutingKey.Should().Be(EventRoutingKeys.CustomerMissionCompleted);

        publishedMessage.Should().NotBeNull();
        publishedMessage!.EventId.Should().Be(response.Data.EventId);
        publishedMessage.EventType.Should().Be(EventTypeCodes.CustomerMissionCompleted);
        publishedMessage.RoutingKey.Should().Be(EventRoutingKeys.CustomerMissionCompleted);

        using var document = JsonDocument.Parse(publishedMessage.Body);
        var root = document.RootElement;
        root.EnumerateObject().Should().HaveCount(5);
        root.GetProperty("eventId").GetGuid().Should().Be(response.Data.EventId);
        root.GetProperty("eventType").GetString().Should().Be(EventTypeCodes.CustomerMissionCompleted);
        root.GetProperty("eventVersion").GetInt32().Should().Be(1);
        root.GetProperty("occurredAt").GetDateTime().Should().Be(FixedNow.UtcDateTime);

        var payload = root.GetProperty("payload");
        payload.GetProperty("customerId").GetGuid().Should().Be(customerId);
        payload.GetProperty("missionCode").GetString().Should().Be("DAILY_LOGIN_7_DAYS");
        payload.GetProperty("missionCategory").GetString().Should().Be("ENGAGEMENT");
        payload.GetProperty("isFirstCompletion").GetBoolean().Should().BeTrue();
        payload.GetProperty("completionCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task PublishCustomerMissionCompleted_OutsideDevelopment_ReturnsNotFoundWithoutPublishing()
    {
        var controller = CreateController(Environments.Production);

        var result = await controller.PublishCustomerMissionCompleted(
            ValidRequest(),
            CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
        _publisher.Verify(
            publisher => publisher.PublishAsync(
                It.IsAny<OutgoingEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishCustomerMissionCompleted_InvalidRequest_ReturnsValidationErrorWithoutPublishing()
    {
        var controller = CreateController(Environments.Development);
        var request = ValidRequest();
        request.CompletionCount = 0;

        var result = await controller.PublishCustomerMissionCompleted(
            request,
            CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().BeOfType<Api.Dtos.Responses.ApiErrorResponseDto>()
            .Which.Error.Details.Should().ContainSingle(error =>
                error.Field == "completionCount" &&
                error.Code == "VALUE_INVALID");
        _publisher.Verify(
            publisher => publisher.PublishAsync(
                It.IsAny<OutgoingEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Controller_IsExplicitlyAnonymous()
    {
        typeof(DevelopmentEventsController)
            .GetCustomAttribute<AllowAnonymousAttribute>()
            .Should().NotBeNull();
    }

    private DevelopmentEventsController CreateController(string environmentName)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.EnvironmentName).Returns(environmentName);

        return new DevelopmentEventsController(
            _publisher.Object,
            new CustomerMissionCompletedRequestDtoValidator(),
            CreateValidationErrorMapper(),
            new FixedTimeProvider(FixedNow),
            environment.Object);
    }

    private static CustomerMissionCompletedRequestDto ValidRequest() => new()
    {
        CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        MissionCode = "DAILY_LOGIN_7_DAYS",
        MissionCategory = "ENGAGEMENT",
        IsFirstCompletion = true,
        CompletionCount = 1
    };

    private static ValidationErrorMapper CreateValidationErrorMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddScoped<ApiMessageResolver>();
        services.AddScoped<ValidationErrorMapper>();
        return services.BuildServiceProvider().GetRequiredService<ValidationErrorMapper>();
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
