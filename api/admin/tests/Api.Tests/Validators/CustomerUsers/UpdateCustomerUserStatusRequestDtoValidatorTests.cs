using Api.Dtos.Requests.CustomerUsers;
using Api.Validators.CustomerUsers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.CustomerUsers;

public sealed class UpdateCustomerUserStatusRequestDtoValidatorTests
{
    [Theory]
    [InlineData("ENABLE")]
    [InlineData(" disable ")]
    public void SupportedStatus_HasNoErrors(string status)
    {
        new UpdateCustomerUserStatusRequestDtoValidator()
            .TestValidate(new UpdateCustomerUserStatusRequestDto { Status = status })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null, "STATUS_REQUIRED")]
    [InlineData("", "STATUS_REQUIRED")]
    [InlineData("LOCKED", "STATUS_INVALID")]
    public void InvalidStatus_ReturnsExpectedError(string? status, string errorCode)
    {
        var result = new UpdateCustomerUserStatusRequestDtoValidator().TestValidate(
            new UpdateCustomerUserStatusRequestDto { Status = status });

        result.ShouldHaveValidationErrorFor("status").WithErrorCode(errorCode);
    }
}
