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
    public void Validate_NameAboveMaxLength_ReturnsLengthInvalidError()
    {
        // Chưa có test cho nhánh "quá dài" (chỉ mới test "quá ngắn")
        var request = ValidRequest();
        request.Name = new string('a', 50); // max = 49

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("name").WithErrorCode("TIER_NAME_LENGTH_INVALID");
    }

    [Theory]
    [InlineData(3)]  // min boundary
    [InlineData(49)] // max boundary
    public void Validate_NameAtLengthBoundary_HasNoError(int length)
    {
        var request = ValidRequest();
        request.Name = new string('a', length);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("name");
    }

    [Theory]
    [InlineData(1)]  // min boundary
    [InlineData(12)] // max boundary
    public void Validate_CycleMonthAtBoundary_HasNoError(int cycleMonth)
    {
        var request = ValidRequest();
        request.CycleMonth = cycleMonth;

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("cycleMonth");
    }

    [Fact]
    public void Validate_PriorityAtMinBoundary_HasNoError()
    {
        // Priority phải > 0, nên 1 là boundary hợp lệ nhỏ nhất
        var request = ValidRequest();
        request.Priority = 1;

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("priority");
    }

    [Fact]
    public void Validate_PointsRequiredAtZero_HasNoError()
    {
        // PointsRequired chỉ reject khi < 0, nên 0 phải hợp lệ (chưa có test dùng >= 0 boundary)
        var request = ValidRequest();
        request.PointsRequired = 0;

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("pointsRequired");
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