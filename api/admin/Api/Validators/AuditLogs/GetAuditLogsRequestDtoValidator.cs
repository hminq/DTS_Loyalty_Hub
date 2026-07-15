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
            .WithMessage("Page must be greater than or equal to 1.")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .WithMessage("Page size must be between 1 and 100.")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.EntityType)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(100)
            .WithErrorCode("ENTITY_TYPE_TOO_LONG")
            .WithMessage("Entity type must be 100 characters or fewer.")
            .When(request => request.EntityType is not null)
            .OverridePropertyName("entityType");

        RuleFor(request => request.Action)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(50)
            .WithErrorCode("ACTION_TOO_LONG")
            .WithMessage("Action must be 50 characters or fewer.")
            .When(request => request.Action is not null)
            .OverridePropertyName("action");

        RuleFor(request => request)
            .Must(request => !request.FromDate.HasValue ||
                              !request.ToDate.HasValue ||
                              request.FromDate <= request.ToDate)
            .WithErrorCode("AUDIT_LOG_DATE_RANGE_INVALID")
            .WithMessage("From date must be earlier than or equal to to date.")
            .OverridePropertyName("fromDate");
    }
}