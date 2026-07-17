using Api.Dtos.Requests.Roles;
using Api.Validators.Roles;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.Roles;

public sealed class RoleRequestDtoValidatorTests
{
    [Fact]
    public void Create_ValidRequest_HasNoErrors()
    {
        var request = new CreateRoleRequestDto
        {
            Name = "Campaign Manager",
            PermissionIds = [Guid.NewGuid(), Guid.NewGuid()]
        };

        var result = new CreateRoleRequestDtoValidator().TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_MissingNameAndPermissions_ReturnsRequiredErrors()
    {
        var request = new CreateRoleRequestDto();

        var result = new CreateRoleRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("name").WithErrorCode("ROLE_NAME_REQUIRED");
        result.ShouldHaveValidationErrorFor("permissionIds")
            .WithErrorCode("ROLE_PERMISSION_IDS_REQUIRED");
    }

    [Fact]
    public void Create_DuplicatedPermissions_ReturnsDuplicatedError()
    {
        var permissionId = Guid.NewGuid();
        var request = new CreateRoleRequestDto
        {
            Name = "Manager",
            PermissionIds = [permissionId, permissionId]
        };

        var result = new CreateRoleRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("permissionIds")
            .WithErrorCode("ROLE_PERMISSION_DUPLICATED");
    }

    [Fact]
    public void Create_EmptyPermissionId_ReturnsRequiredError()
    {
        var request = new CreateRoleRequestDto
        {
            Name = "Manager",
            PermissionIds = [Guid.Empty]
        };

        var result = new CreateRoleRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("PermissionIds[0]")
            .WithErrorCode("PERMISSION_ID_REQUIRED");
    }

    [Fact]
    public void Update_DuplicatedPermissions_ReturnsDuplicatedError()
    {
        var permissionId = Guid.NewGuid();
        var request = new UpdateRoleRequestDto
        {
            Name = "Manager",
            PermissionIds = [permissionId, permissionId]
        };

        var result = new UpdateRoleRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("permissionIds")
            .WithErrorCode("ROLE_PERMISSION_DUPLICATED");
    }

    [Fact]
    public void GetPaged_InvalidPagingAndKeyword_ReturnExpectedErrors()
    {
        var request = new GetRolesRequestDto
        {
            Page = 0,
            PageSize = 101,
            Keyword = new string('a', 101)
        };

        var result = new GetRolesRequestDtoValidator().TestValidate(request);

        result.ShouldHaveValidationErrorFor("page").WithErrorCode("PAGE_INVALID");
        result.ShouldHaveValidationErrorFor("pageSize").WithErrorCode("PAGE_SIZE_INVALID");
        result.ShouldHaveValidationErrorFor("keyword").WithErrorCode("KEYWORD_TOO_LONG");
    }
}
