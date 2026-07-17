using Api.Dtos.Requests.Auth;
using Api.Validators.Auth;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.Auth;

public sealed class LoginRequestDtoValidatorTests
{
    private readonly LoginRequestDtoValidator _sut = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var request = new LoginRequestDto { Username = "admin", Password = "Password123" };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_UsernameIsEmpty_ReturnsRequiredError(string? username)
    {
        var request = new LoginRequestDto { Username = username, Password = "Password123" };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("username").WithErrorCode("USERNAME_REQUIRED");
    }

    [Fact]
    public void Validate_UsernameExceedsMaximumLength_ReturnsTooLongError()
    {
        var request = new LoginRequestDto
        {
            Username = new string('a', 51),
            Password = "Password123"
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("username").WithErrorCode("USERNAME_TOO_LONG");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_PasswordIsEmpty_ReturnsRequiredError(string? password)
    {
        var request = new LoginRequestDto { Username = "admin", Password = password };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("password").WithErrorCode("PASSWORD_REQUIRED");
    }

    [Fact]
    public void Validate_PasswordExceedsMaximumLength_ReturnsTooLongError()
    {
        var request = new LoginRequestDto
        {
            Username = "admin",
            Password = new string('a', 201)
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("password").WithErrorCode("PASSWORD_TOO_LONG");
    }
}
