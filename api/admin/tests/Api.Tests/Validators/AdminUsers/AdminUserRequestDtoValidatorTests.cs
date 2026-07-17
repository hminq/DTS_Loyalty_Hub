using Api.Dtos.Requests.AdminUsers;
using Api.Validators.AdminUsers;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.AdminUsers;

public sealed class AdminUserRequestDtoValidatorTests
{
    [Fact]
    public void Create_ValidRequest_HasNoErrors()
    {
        var request = ValidCreateRequest();

        var result = new CreateAdminUserRequestDtoValidator().TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_MissingRequiredFields_ReturnsCamelCaseErrors()
    {
        var request = new CreateAdminUserRequestDto();

        var result = new CreateAdminUserRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("username").WithErrorCode("USERNAME_REQUIRED");
        result.ShouldHaveValidationErrorFor("email").WithErrorCode("EMAIL_REQUIRED");
        result.ShouldHaveValidationErrorFor("password").WithErrorCode("PASSWORD_REQUIRED");
        result.ShouldHaveValidationErrorFor("roleId").WithErrorCode("ROLE_ID_REQUIRED");
    }

    [Fact]
    public void Create_InvalidFormatsAndLengths_ReturnExpectedErrors()
    {
        var request = ValidCreateRequest();
        request.Email = "invalid";
        request.Password = "short";
        request.FullName = new string('a', 51);
        request.PhoneNumber = new string('1', 16);

        var result = new CreateAdminUserRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("email").WithErrorCode("EMAIL_INVALID");
        result.ShouldHaveValidationErrorFor("password").WithErrorCode("PASSWORD_TOO_SHORT");
        result.ShouldHaveValidationErrorFor("fullName").WithErrorCode("FULL_NAME_TOO_LONG");
        result.ShouldHaveValidationErrorFor("phoneNumber").WithErrorCode("PHONE_NUMBER_TOO_LONG");
    }

    [Fact]
    public void Update_ValidRequest_HasNoErrors()
    {
        var request = new UpdateAdminUserRequestDto
        {
            Email = "admin@example.com",
            FullName = "Admin User",
            PhoneNumber = "0123456789",
            RoleId = Guid.NewGuid()
        };

        var result = new UpdateAdminUserRequestDtoValidator().TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_InvalidEmailAndRole_ReturnExpectedErrors()
    {
        var request = new UpdateAdminUserRequestDto { Email = "invalid", RoleId = Guid.Empty };

        var result = new UpdateAdminUserRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("email").WithErrorCode("EMAIL_INVALID");
        result.ShouldHaveValidationErrorFor("roleId").WithErrorCode("ROLE_ID_REQUIRED");
    }

    [Theory]
    [InlineData("ENABLE")]
    [InlineData("disable")]
    [InlineData(" ENABLE ")]
    public void UpdateStatus_DefinedStatus_HasNoErrors(string status)
    {
        var request = new UpdateAdminUserStatusRequestDto { Status = status };

        var result = new UpdateAdminUserStatusRequestDtoValidator().TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null, "STATUS_REQUIRED")]
    [InlineData("", "STATUS_REQUIRED")]
    [InlineData("LOCKED", "STATUS_INVALID")]
    public void UpdateStatus_InvalidStatus_ReturnsExpectedError(string? status, string errorCode)
    {
        var request = new UpdateAdminUserStatusRequestDto { Status = status };

        var result = new UpdateAdminUserStatusRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("status").WithErrorCode(errorCode);
    }

    [Fact]
    public void GetPaged_InvalidPagingAndFilters_ReturnExpectedErrors()
    {
        var request = new GetAdminUsersRequestDto
        {
            Page = 0,
            PageSize = 101,
            Keyword = new string('a', 101),
            Status = new string('a', 26)
        };

        var result = new GetAdminUsersRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("page").WithErrorCode("PAGE_INVALID");
        result.ShouldHaveValidationErrorFor("pageSize").WithErrorCode("PAGE_SIZE_INVALID");
        result.ShouldHaveValidationErrorFor("keyword").WithErrorCode("KEYWORD_TOO_LONG");
        result.ShouldHaveValidationErrorFor("status").WithErrorCode("STATUS_TOO_LONG");
    }

    private static CreateAdminUserRequestDto ValidCreateRequest() => new()
    {
        Username = "admin",
        Email = "admin@example.com",
        Password = "Password123",
        FullName = "Admin User",
        PhoneNumber = "0123456789",
        RoleId = Guid.NewGuid()
    };
}
