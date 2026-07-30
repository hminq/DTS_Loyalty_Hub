using System.Text.Json;
using Api.Dtos.Requests.Campaigns;
using Api.Validators.Campaigns;
using FluentAssertions;

namespace Api.Tests.Validators.Campaigns;

public sealed class CampaignWriteRequestDtoValidatorTests
{
    private readonly CampaignWriteRequestDtoValidator _campaignValidator = new();
    private readonly CreateCampaignRequestDtoValidator _createCampaignValidator = new();
    private readonly CampaignActionWriteRequestDtoValidator _actionValidator = new();

    [Fact]
    public async Task CampaignRequest_ValidShape_Passes()
    {
        var result = await _campaignValidator.ValidateAsync(ValidCampaignRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CampaignRequest_EndBeforeStart_ReturnsDateRangeError()
    {
        var request = ValidCampaignRequest() with
        {
            EndDate = new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero)
        };

        var result = await _campaignValidator.ValidateAsync(request);

        result.Errors.Should().Contain(error =>
            error.PropertyName == "endDate" &&
            error.ErrorCode == "CAMPAIGN_DATE_RANGE_INVALID");
    }

    [Fact]
    public async Task CampaignRequest_SessionLimitAboveTotal_ReturnsLimitError()
    {
        var request = ValidCampaignRequest() with
        {
            UserLimitTotal = 1,
            UserLimitSession = 2
        };

        var result = await _campaignValidator.ValidateAsync(request);

        result.Errors.Should().Contain(error =>
            error.PropertyName == "userLimitSession" &&
            error.ErrorCode == "CAMPAIGN_LIMIT_INVALID");
    }

    [Fact]
    public async Task CampaignRequest_NonObjectCondition_ReturnsConditionError()
    {
        var request = ValidCampaignRequest() with { Condition = Json("[]") };

        var result = await _campaignValidator.ValidateAsync(request);

        result.Errors.Should().Contain(error =>
            error.PropertyName == "condition" &&
            error.ErrorCode == "CAMPAIGN_CONDITION_INVALID");
    }

    [Theory]
    [InlineData("0 42 15 * * ?.")]
    [InlineData("0 05 15 * * ?")]
    [InlineData("0 42 15 ? * WED,MON")]
    public async Task CampaignRequest_UnsupportedScheduleCron_ReturnsScheduleError(
        string scheduleCron)
    {
        var request = ValidCampaignRequest() with { ScheduleCron = scheduleCron };

        var result = await _campaignValidator.ValidateAsync(request);

        result.Errors.Should().Contain(error =>
            error.PropertyName == "scheduleCron" &&
            error.ErrorCode == "CAMPAIGN_SCHEDULE_INVALID");
    }

    [Theory]
    [InlineData("0 0 9 15 * ?")]
    [InlineData("0 0 9 1,5,6 * ?")]
    [InlineData("0 0 9 L * ?")]
    public async Task CampaignRequest_MonthlyScheduleCron_PassesScheduleValidation(
        string scheduleCron)
    {
        var request = ValidCampaignRequest() with { ScheduleCron = scheduleCron };

        var result = await _campaignValidator.ValidateAsync(request);

        result.Errors.Should().NotContain(error =>
            error.PropertyName == "scheduleCron");
    }

    [Fact]
    public async Task CreateCampaignRequest_ValidShapeWithAction_Passes()
    {
        var result = await _createCampaignValidator.ValidateAsync(ValidCreateCampaignRequest());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCampaignRequest_MissingOrEmptyActions_ReturnsRequiredError(bool useEmptyArray)
    {
        var request = ValidCreateCampaignRequest() with
        {
            Actions = useEmptyArray ? [] : null
        };

        var result = await _createCampaignValidator.ValidateAsync(request);

        result.Errors.Should().Contain(error =>
            error.PropertyName == "actions" &&
            error.ErrorCode == "CAMPAIGN_ACTIONS_REQUIRED");
    }

    [Fact]
    public async Task CreateCampaignRequest_InvalidNestedAction_ReturnsNestedField()
    {
        var request = ValidCreateCampaignRequest() with
        {
            Actions =
            [
                new CampaignActionWriteRequestDto
                {
                    ActionType = "ISSUE_POINT",
                    ActionConfig = Json("{}"),
                    ExecuteOrder = 0
                }
            ]
        };

        var result = await _createCampaignValidator.ValidateAsync(request);

        result.Errors.Should().Contain(error =>
            error.PropertyName == "actions[0].executeOrder" &&
            error.ErrorCode == "CAMPAIGN_ACTION_ORDER_INVALID");
    }

    [Fact]
    public async Task ActionRequest_ValidShape_Passes()
    {
        var request = new CampaignActionWriteRequestDto
        {
            ActionType = "ISSUE_POINT",
            ActionConfig = Json(
                """{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50}}"""),
            ExecuteOrder = 1,
            TotalCount = 10,
            SessionCount = 5
        };

        var result = await _actionValidator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ActionRequest_InvalidOrderAndLimits_ReturnsExpectedErrors()
    {
        var request = new CampaignActionWriteRequestDto
        {
            ActionType = "ISSUE_POINT",
            ActionConfig = Json("{}"),
            ExecuteOrder = 0,
            TotalCount = 1,
            SessionCount = 2
        };

        var result = await _actionValidator.ValidateAsync(request);

        result.Errors.Should().Contain(error =>
            error.PropertyName == "executeOrder" &&
            error.ErrorCode == "CAMPAIGN_ACTION_ORDER_INVALID");
        result.Errors.Should().Contain(error =>
            error.PropertyName == "sessionCount" &&
            error.ErrorCode == "CAMPAIGN_LIMIT_INVALID");
    }

    private static CampaignWriteRequestDto ValidCampaignRequest() => new()
    {
        CampaignName = "Normal registration reward",
        EventTypeVersionId = Guid.NewGuid(),
        Condition = Json(
            """{"all":[{"field":"source","operator":"EQUALS","value":"NORMAL"}]}"""),
        StartDate = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
        EndDate = new DateTimeOffset(2026, 8, 31, 0, 0, 0, TimeSpan.Zero),
        ScheduleCron = "0 0 2 * * ?",
        DurationHour = 2,
        UserLimitTotal = 1,
        UserLimitSession = 1
    };

    private static CreateCampaignRequestDto ValidCreateCampaignRequest() => new()
    {
        CampaignName = "Normal registration reward",
        EventTypeVersionId = Guid.NewGuid(),
        Condition = Json(
            """{"all":[{"field":"source","operator":"EQUALS","value":"NORMAL"}]}"""),
        StartDate = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
        EndDate = new DateTimeOffset(2026, 8, 31, 0, 0, 0, TimeSpan.Zero),
        ScheduleCron = "0 0 2 * * ?",
        DurationHour = 2,
        UserLimitTotal = 1,
        UserLimitSession = 1,
        Actions =
        [
            new CampaignActionWriteRequestDto
            {
                ActionType = "ISSUE_POINT",
                ActionConfig = Json(
                    """{"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50}}"""),
                ExecuteOrder = 1
            }
        ]
    };

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
