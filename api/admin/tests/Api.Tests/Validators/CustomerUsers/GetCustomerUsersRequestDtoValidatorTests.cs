using Api.Dtos.Requests.CustomerUsers;
using Api.Validators.CustomerUsers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.CustomerUsers;

public sealed class GetCustomerUsersRequestDtoValidatorTests
{
    [Fact]
    public void ValidFilters_HaveNoErrors()
    {
        var request = new GetCustomerUsersRequestDto
        {
            Page = 1,
            PageSize = 100,
            Keyword = "customer",
            Status = " enable ",
            TierId = Guid.NewGuid()
        };

        var result = new GetCustomerUsersRequestDtoValidator().TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidPagingAndFilters_ReturnCamelCaseErrors()
    {
        var request = new GetCustomerUsersRequestDto
        {
            Page = 0,
            PageSize = 101,
            Keyword = new string('a', 101),
            Status = "LOCKED",
            TierId = Guid.Empty
        };

        var result = new GetCustomerUsersRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("page").WithErrorCode("PAGE_INVALID");
        result.ShouldHaveValidationErrorFor("pageSize").WithErrorCode("PAGE_SIZE_INVALID");
        result.ShouldHaveValidationErrorFor("keyword").WithErrorCode("KEYWORD_TOO_LONG");
        result.ShouldHaveValidationErrorFor("status").WithErrorCode("STATUS_INVALID");
        result.ShouldHaveValidationErrorFor("tierId").WithErrorCode("TIER_ID_INVALID");
    }

    [Fact]
    public void OverlongStatus_StopsAtLengthError()
    {
        var request = new GetCustomerUsersRequestDto
        {
            Status = new string('a', 26)
        };

        var result = new GetCustomerUsersRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("status").WithErrorCode("STATUS_TOO_LONG");
        result.Errors.Should().NotContain(error => error.ErrorCode == "STATUS_INVALID");
    }
}
