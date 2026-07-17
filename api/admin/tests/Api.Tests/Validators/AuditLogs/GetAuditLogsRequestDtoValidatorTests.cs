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
}
