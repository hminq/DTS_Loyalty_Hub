using Api.Dtos.Requests.AuditLogs;
using Api.Validators.AuditLogs;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators.AuditLogs;

public sealed class GetAuditLogsRequestDtoValidatorTests
{
    private readonly GetAuditLogsRequestDtoValidator _sut = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var request = new GetAuditLogsRequestDto
        {
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 1, 31),
            EntityType = "VoucherDefinition",
            Action = "CREATE"
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidPagingAndLengths_ReturnExpectedErrors()
    {
        var request = new GetAuditLogsRequestDto
        {
            Page = 0,
            PageSize = 101,
            EntityType = new string('a', 101),
            Action = new string('a', 51)
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("page").WithErrorCode("PAGE_INVALID");
        result.ShouldHaveValidationErrorFor("pageSize").WithErrorCode("PAGE_SIZE_INVALID");
        result.ShouldHaveValidationErrorFor("entityType").WithErrorCode("ENTITY_TYPE_TOO_LONG");
        result.ShouldHaveValidationErrorFor("action").WithErrorCode("ACTION_TOO_LONG");
    }

    [Fact]
    public void Validate_FromDateAfterToDate_ReturnsDateRangeError()
    {
        var request = new GetAuditLogsRequestDto
        {
            FromDate = new DateTime(2026, 2, 1),
            ToDate = new DateTime(2026, 1, 31)
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("fromDate")
            .WithErrorCode("AUDIT_LOG_DATE_RANGE_INVALID");
    }

    [Fact]
    public void Validate_FromDateEqualsToDate_HasNoError()
    {
        // Boundary: rule chỉ reject khi FromDate > ToDate, bằng nhau phải hợp lệ
        var sameDate = new DateTime(2026, 1, 15);
        var request = new GetAuditLogsRequestDto { FromDate = sameDate, ToDate = sameDate };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("fromDate");
    }

    [Theory]
    [InlineData(1, 1)]     // Page min boundary, PageSize min boundary
    [InlineData(1, 100)]   // PageSize max boundary
    public void Validate_PagingAtBoundary_HasNoError(int page, int pageSize)
    {
        var request = new GetAuditLogsRequestDto { Page = page, PageSize = pageSize };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("page");
        result.ShouldNotHaveValidationErrorFor("pageSize");
    }

    [Fact]
    public void Validate_EntityTypeAndActionAtMaxLength_HasNoError()
    {
        // Boundary: đúng 100/50 ký tự phải hợp lệ, không bị off-by-one reject nhầm
        var request = new GetAuditLogsRequestDto
        {
            EntityType = new string('a', 100),
            Action = new string('a', 50)
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("entityType");
        result.ShouldNotHaveValidationErrorFor("action");
    }

    [Fact]
    public void Validate_OnlyFromDateProvided_HasNoDateRangeError()
    {
        // Chỉ có 1 trong 2 mốc thời gian -> điều kiện so sánh khoảng ngày không áp dụng
        var request = new GetAuditLogsRequestDto { FromDate = new DateTime(2026, 1, 1), ToDate = null };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("fromDate");
    }
}