using Api.Dtos.Requests.Auth;
using Api.Validators;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators;

public sealed class RegisterRequestDtoValidatorTests
{
    private readonly RegisterRequestDtoValidator _validator = new();

    [Fact]
    public void ValidVietnameseProfile_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("A", "FULLNAME_INVALID_LENGTH")]
    [InlineData("Customer 123", "FULLNAME_INVALID_FORMAT")]
    [InlineData("Customer@Name", "FULLNAME_INVALID_FORMAT")]
    public void InvalidFullName_ReturnsExpectedError(string fullName, string errorCode)
    {
        var request = ValidRequest();
        request.FullName = fullName;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("fullName").WithErrorCode(errorCode);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("+12345678")]
    [InlineData("01234abcde")]
    [InlineData("0123 456 789")]
    [InlineData("+123456789012345")]
    public void InvalidPhone_ReturnsFormatError(string phone)
    {
        var request = ValidRequest();
        request.Phone = phone;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("phone").WithErrorCode("PHONE_INVALID_FORMAT");
    }

    private static RegisterRequestDto ValidRequest() => new()
    {
        Username = "customer01",
        Email = "customer@example.com",
        Password = "Password1",
        FullName = "Nguyễn Minh Anh",
        Phone = "+84901234567"
    };
}
