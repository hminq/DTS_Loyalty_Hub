using Api.Dtos.Requests.AuditLogs;
using FluentValidation;

namespace Api.Validators.AuditLogs;

public sealed class GetAuditLogsRequestDtoValidator : AbstractValidator<GetAuditLogsRequestDto>
{
    public GetAuditLogsRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.EntityType)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(100)
            .WithErrorCode("ENTITY_TYPE_TOO_LONG")
            .When(request => request.EntityType is not null)
            .OverridePropertyName("entityType");

        RuleFor(request => request.Action)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(50)
            .WithErrorCode("ACTION_TOO_LONG")
            .When(request => request.Action is not null)
            .OverridePropertyName("action");

        RuleFor(request => request)
            .Must(request => !request.FromDate.HasValue ||
                              !request.ToDate.HasValue ||
                              request.FromDate <= request.ToDate)
            .WithErrorCode("AUDIT_LOG_DATE_RANGE_INVALID")
            .OverridePropertyName("fromDate");
    }
}
