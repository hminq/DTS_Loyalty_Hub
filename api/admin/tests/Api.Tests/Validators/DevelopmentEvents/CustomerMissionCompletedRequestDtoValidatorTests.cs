using Api.Dtos.Requests.DevelopmentEvents;
using Api.Validators.DevelopmentEvents;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.DevelopmentEvents;

public sealed class CustomerMissionCompletedRequestDtoValidatorTests
{
    private readonly CustomerMissionCompletedRequestDtoValidator _validator = new();

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MissingRequiredFields_ReturnsCamelCaseErrors()
    {
        var result = _validator.TestValidate(new CustomerMissionCompletedRequestDto());

        result.ShouldHaveValidationErrorFor("customerId")
            .WithErrorCode("CUSTOMER_ID_REQUIRED");
        result.ShouldHaveValidationErrorFor("missionCode")
            .WithErrorCode("VALUE_INVALID");
        result.ShouldHaveValidationErrorFor("missionCategory")
            .WithErrorCode("VALUE_INVALID");
        result.ShouldHaveValidationErrorFor("isFirstCompletion")
            .WithErrorCode("VALUE_INVALID");
        result.ShouldHaveValidationErrorFor("completionCount")
            .WithErrorCode("VALUE_INVALID");
    }

    private static CustomerMissionCompletedRequestDto ValidRequest() => new()
    {
        CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        MissionCode = "DAILY_LOGIN_7_DAYS",
        MissionCategory = "ENGAGEMENT",
        IsFirstCompletion = false,
        CompletionCount = 2
    };
}
