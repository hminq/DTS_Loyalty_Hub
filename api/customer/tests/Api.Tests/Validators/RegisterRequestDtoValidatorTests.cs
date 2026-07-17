using Api.Dtos.Requests.Auth;
using Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators;

public class RegisterRequestDtoValidatorTests
{
    private readonly RegisterRequestDtoValidator _sut = new();

    private static RegisterRequestDto CreateValidDto() => new()
    {
        Username = "john_doe",
        Email = "john.doe@example.com",
        Password = "Pass12",
        FullName = "John Doe",
        Phone = "0987654321"
    };

    [Fact]
    public void Validate_ValidRequest_NoValidationError()
    {
        var dto = CreateValidDto();

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ---------- Username ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_UsernameEmpty_HasRequiredError(string? username)
    {
        var dto = CreateValidDto();
        dto.Username = username;

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("username")
              .WithErrorCode("USERNAME_REQUIRED");
    }

    [Fact]
    public void Validate_UsernameBelowMinLength_HasInvalidLengthError()
    {
        var dto = CreateValidDto();
        dto.Username = new string('a', 4); // min = 5

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("username")
              .WithErrorCode("USERNAME_INVALID_LENGTH");
    }

    [Fact]
    public void Validate_UsernameAboveMaxLength_HasInvalidLengthError()
    {
        var dto = CreateValidDto();
        dto.Username = new string('a', 51); // max = 50

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("username")
              .WithErrorCode("USERNAME_INVALID_LENGTH");
    }

    [Theory]
    [InlineData(5)]  // min boundary
    [InlineData(50)] // max boundary
    public void Validate_UsernameAtLengthBoundary_NoValidationError(int length)
    {
        var dto = CreateValidDto();
        dto.Username = new string('a', length);

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor("username");
    }

    // ---------- Email ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmailEmpty_HasRequiredError(string? email)
    {
        var dto = CreateValidDto();
        dto.Email = email;

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("email")
              .WithErrorCode("EMAIL_REQUIRED");
    }

    [Fact]
    public void Validate_EmailAboveMaxLength_HasInvalidLengthError()
    {
        var dto = CreateValidDto();
        // vẫn là format email hợp lệ nhưng vượt quá 50 ký tự
        dto.Email = new string('a', 45) + "@a.com"; // 51 chars

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("email")
              .WithErrorCode("EMAIL_INVALID_LENGTH");
    }

    [Fact]
    public void Validate_EmailWrongFormatButValidLength_HasInvalidFormatError()
    {
        var dto = CreateValidDto();
        // đủ độ dài (>= 5, <= 50) nên vượt qua rule Length, nhưng không phải email hợp lệ
        // -> đảm bảo rule EmailAddress() thực sự được kiểm tra chứ không bị Length rule "che" mất
        dto.Email = "invalid-email";

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("email")
              .WithErrorCode("EMAIL_INVALID_FORMAT");
    }

    [Fact]
    public void Validate_EmailAtMinLengthBoundary_NoValidationError()
    {
        var dto = CreateValidDto();
        dto.Email = "a@b.co"; // 6 chars, hợp lệ về format và length

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor("email");
    }

    // ---------- Password ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_PasswordEmpty_HasRequiredError(string? password)
    {
        var dto = CreateValidDto();
        dto.Password = password;

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("password")
              .WithErrorCode("PASSWORD_REQUIRED");
    }

    [Fact]
    public void Validate_PasswordBelowMinLength_HasTooShortError()
    {
        var dto = CreateValidDto();
        dto.Password = "Ab1"; // 3 chars, min = 5

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("password")
              .WithErrorCode("PASSWORD_TOO_SHORT");
    }

    [Fact]
    public void Validate_PasswordAboveMaxLength_HasTooLongError()
    {
        var dto = CreateValidDto();
        dto.Password = "Aa1" + new string('a', 198); // 201 chars, max = 200

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("password")
              .WithErrorCode("PASSWORD_TOO_LONG");
    }

    [Fact]
    public void Validate_PasswordMissingUppercase_HasMissingUppercaseError()
    {
        var dto = CreateValidDto();
        // đủ độ dài, có digit, nhưng thiếu chữ hoa
        // -> đảm bảo rule Matches("[A-Z]") thực sự được kiểm tra riêng biệt
        dto.Password = "pass12";

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("password")
              .WithErrorCode("PASSWORD_MISSING_UPPERCASE");
    }

    [Fact]
    public void Validate_PasswordMissingDigit_HasMissingDigitError()
    {
        var dto = CreateValidDto();
        // đủ độ dài, có chữ hoa, nhưng thiếu digit
        // -> đảm bảo rule Matches("[0-9]") thực sự được kiểm tra riêng biệt, độc lập với rule hoa
        dto.Password = "Password";

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("password")
              .WithErrorCode("PASSWORD_MISSING_DIGIT");
    }

    [Fact]
    public void Validate_PasswordAtMinLengthWithValidComplexity_NoValidationError()
    {
        var dto = CreateValidDto();
        dto.Password = "Pass1"; // 5 chars, có hoa + digit -> thỏa mọi rule

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor("password");
    }

    // ---------- FullName ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_FullNameEmpty_HasRequiredError(string? fullName)
    {
        var dto = CreateValidDto();
        dto.FullName = fullName;

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("fullName")
              .WithErrorCode("FULLNAME_REQUIRED");
    }

    [Fact]
    public void Validate_FullNameBelowMinLength_HasInvalidLengthError()
    {
        var dto = CreateValidDto();
        dto.FullName = "Bob"; // 3 chars, min = 5

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("fullName")
              .WithErrorCode("FULLNAME_INVALID_LENGTH");
    }

    [Fact]
    public void Validate_FullNameAboveMaxLength_HasInvalidLengthError()
    {
        var dto = CreateValidDto();
        dto.FullName = new string('a', 51); // max = 50

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("fullName")
              .WithErrorCode("FULLNAME_INVALID_LENGTH");
    }

    // ---------- Phone ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_PhoneEmpty_HasRequiredError(string? phone)
    {
        var dto = CreateValidDto();
        dto.Phone = phone;

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("phone")
              .WithErrorCode("PHONE_REQUIRED");
    }

    [Theory]
    [InlineData("098765432a")]     // chứa ký tự chữ
    [InlineData("0987-654-321")]   // chứa dấu gạch ngang, người dùng hay nhập kiểu này
    [InlineData("12345678")]       // 8 số, dưới min 9 -> phải reject chứ không âm thầm pass
    [InlineData("1234567890123456")] // 16 số, vượt max 15
    [InlineData("++84987654321")]  // 2 dấu +
    public void Validate_PhoneWrongFormat_HasInvalidFormatError(string phone)
    {
        var dto = CreateValidDto();
        dto.Phone = phone;

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("phone")
              .WithErrorCode("PHONE_INVALID_FORMAT");
    }

    [Theory]
    [InlineData("123456789")]        // 9 số, boundary min, không có +
    [InlineData("123456789012345")]  // 15 số, boundary max
    [InlineData("+84987654321")]     // có dấu + hợp lệ
    public void Validate_PhoneValidFormat_NoValidationError(string phone)
    {
        var dto = CreateValidDto();
        dto.Phone = phone;

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor("phone");
    }

    // ---------- Multiple fields ----------

    [Fact]
    public void Validate_AllFieldsEmpty_HasValidationErrorForEveryField()
    {
        var dto = new RegisterRequestDto
        {
            Username = "",
            Email = "",
            Password = "",
            FullName = "",
            Phone = ""
        };

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("username");
        result.ShouldHaveValidationErrorFor("email");
        result.ShouldHaveValidationErrorFor("password");
        result.ShouldHaveValidationErrorFor("fullName");
        result.ShouldHaveValidationErrorFor("phone");
    }
}