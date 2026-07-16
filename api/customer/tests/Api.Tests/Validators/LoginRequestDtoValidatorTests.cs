using Api.Dtos.Requests.Auth;
using Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators;

public class LoginRequestDtoValidatorTests
{
    private readonly LoginRequestDtoValidator _sut = new();

    [Fact]
    public void Validate_ValidRequest_NoValidationError()
    {
        var dto = new LoginRequestDto { Username = "john_doe", Password = "Pass123" };

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_UsernameEmpty_HasValidationError(string? username)
    {
        var dto = new LoginRequestDto { Username = username, Password = "Pass123" };

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("username")
              .WithErrorCode("USERNAME_REQUIRED");
    }

    [Fact]
    public void Validate_UsernameTooLong_HasValidationError()
    {
        var dto = new LoginRequestDto { Username = new string('a', 51), Password = "Pass123" };

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("username")
              .WithErrorCode("USERNAME_TOO_LONG");
    }

    [Fact]
    public void Validate_UsernameAtMaxLength_NoValidationError()
    {
        var dto = new LoginRequestDto { Username = new string('a', 50), Password = "Pass123" };

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor("username");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_PasswordEmpty_HasValidationError(string? password)
    {
        var dto = new LoginRequestDto { Username = "john_doe", Password = password };

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("password")
              .WithErrorCode("PASSWORD_REQUIRED");
    }

    [Fact]
    public void Validate_PasswordTooLong_HasValidationError()
    {
        var dto = new LoginRequestDto { Username = "john_doe", Password = new string('a', 201) };

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("password")
              .WithErrorCode("PASSWORD_TOO_LONG");
    }

    [Fact]
    public void Validate_PasswordAtMaxLength_NoValidationError()
    {
        var dto = new LoginRequestDto { Username = "john_doe", Password = new string('a', 200) };

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor("password");
    }

    [Fact]
    public void Validate_UsernameAndPasswordEmpty_HasValidationErrorForBoth()
    {
        var dto = new LoginRequestDto { Username = "", Password = "" };

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("username");
        result.ShouldHaveValidationErrorFor("password");
    }

    [Fact]
    public void Validate_PropertyNamesAreOverriddenToCamelCase()
    {
        var dto = new LoginRequestDto { Username = "", Password = "" };

        var result = _sut.TestValidate(dto);

        result.Errors.Should().Contain(e => e.PropertyName == "username");
        result.Errors.Should().Contain(e => e.PropertyName == "password");
    }
}
