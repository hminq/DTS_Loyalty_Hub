using Api.Dtos.Requests.CustomerUsers;
using Api.Validators.CustomerUsers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.CustomerUsers;

public sealed class UpdateCustomerUserRequestDtoValidatorTests
{
    [Fact]
    public void ValidProfile_HasNoErrors()
    {
        var request = new UpdateCustomerUserRequestDto
        {
            Email = "customer@example.com",
            FullName = "Customer Name",
            PhoneNumber = "0123456789"
        };

        new UpdateCustomerUserRequestDtoValidator()
            .TestValidate(request)
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidProfile_ReturnsCamelCaseFields()
    {
        var request = new UpdateCustomerUserRequestDto
        {
            Email = "invalid",
            FullName = new string('a', 51),
            PhoneNumber = new string('1', 16)
        };

        var result = new UpdateCustomerUserRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("email").WithErrorCode("EMAIL_INVALID");
        result.ShouldHaveValidationErrorFor("fullName").WithErrorCode("FULL_NAME_TOO_LONG");
        result.ShouldHaveValidationErrorFor("phoneNumber").WithErrorCode("PHONE_NUMBER_TOO_LONG");
    }

    [Fact]
    public void EmptyEmail_StopsAtRequiredError()
    {
        var result = new UpdateCustomerUserRequestDtoValidator().TestValidate(
            new UpdateCustomerUserRequestDto { Email = string.Empty });

        result.ShouldHaveValidationErrorFor("email").WithErrorCode("EMAIL_REQUIRED");
        result.Errors.Should().NotContain(error => error.ErrorCode == "EMAIL_INVALID");
    }

    [Theory]
    [InlineData("A", "FULL_NAME_TOO_SHORT")]
    [InlineData("Customer 123", "FULL_NAME_INVALID")]
    [InlineData("Customer@Name", "FULL_NAME_INVALID")]
    public void InvalidFullName_ReturnsExpectedError(string fullName, string errorCode)
    {
        var result = new UpdateCustomerUserRequestDtoValidator().TestValidate(
            new UpdateCustomerUserRequestDto
            {
                Email = "customer@example.com",
                FullName = fullName
            });

        result.ShouldHaveValidationErrorFor("fullName").WithErrorCode(errorCode);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("+12345678")]
    [InlineData("01234abcde")]
    [InlineData("0123 456 789")]
    public void InvalidPhoneNumber_ReturnsFormatError(string phoneNumber)
    {
        var result = new UpdateCustomerUserRequestDtoValidator().TestValidate(
            new UpdateCustomerUserRequestDto
            {
                Email = "customer@example.com",
                PhoneNumber = phoneNumber
            });

        result.ShouldHaveValidationErrorFor("phoneNumber").WithErrorCode("PHONE_NUMBER_INVALID");
    }

    [Fact]
    public void VietnameseNameAndInternationalPhone_HaveNoErrors()
    {
        var result = new UpdateCustomerUserRequestDtoValidator().TestValidate(
            new UpdateCustomerUserRequestDto
            {
                Email = "customer@example.com",
                FullName = "Nguyễn Thị Minh Anh",
                PhoneNumber = "+84901234567"
            });

        result.ShouldNotHaveAnyValidationErrors();
    }
}
