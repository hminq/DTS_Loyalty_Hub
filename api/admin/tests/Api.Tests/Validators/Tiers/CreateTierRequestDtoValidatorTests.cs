using Api.Dtos.Requests.Tiers;
using Api.Validators.Tiers;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.Tiers;

public sealed class CreateTierRequestDtoValidatorTests
{
    private readonly CreateTierRequestDtoValidator _sut = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var request = new CreateTierRequestDto
        {
            Name = "Gold",
            PointsRequired = 1000,
            CycleMonth = 12,
            Priority = 1
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "TIER_NAME_REQUIRED")]
    [InlineData("ab", "TIER_NAME_LENGTH_INVALID")]
    public void Validate_InvalidName_ReturnsExpectedError(string name, string errorCode)
    {
        var request = ValidRequest();
        request.Name = name;

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("name").WithErrorCode(errorCode);
    }

    [Fact]
    public void Validate_InvalidNumbers_ReturnExpectedErrors()
    {
        var request = new CreateTierRequestDto
        {
            Name = "Gold",
            PointsRequired = -1,
            CycleMonth = 13,
            Priority = 0
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("pointsRequired")
            .WithErrorCode("TIER_POINTS_REQUIRED_INVALID");
        result.ShouldHaveValidationErrorFor("cycleMonth")
            .WithErrorCode("TIER_CYCLE_MONTH_INVALID");
        result.ShouldHaveValidationErrorFor("priority").WithErrorCode("TIER_PRIORITY_INVALID");
    }

    private static CreateTierRequestDto ValidRequest() => new()
    {
        Name = "Gold",
        PointsRequired = 1000,
        CycleMonth = 12,
        Priority = 1
    };
}
