using System.Text.RegularExpressions;
using Api.Dtos.Requests.EventDefinitions;
using Core.Entities.Constants;
using FluentValidation;
using FluentValidation.Results;

namespace Api.Validators.EventDefinitions;

public sealed partial class GetEventDefinitionsRequestDtoValidator
    : AbstractValidator<GetEventDefinitionsRequestDto>
{
    public GetEventDefinitionsRequestDtoValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("PAGE_INVALID")
            .OverridePropertyName("page");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("PAGE_SIZE_INVALID")
            .OverridePropertyName("pageSize");

        RuleFor(request => request.Keyword)
            .MaximumLength(EventDefinitionSchemaLimits.MaximumKeywordLength)
            .WithErrorCode("KEYWORD_TOO_LONG")
            .When(request => request.Keyword is not null)
            .OverridePropertyName("keyword");

        RuleFor(request => request.Status)
            .Must(EventDefinitionStatuses.IsDefined!)
            .WithErrorCode("STATUS_INVALID")
            .When(request => !string.IsNullOrWhiteSpace(request.Status))
            .OverridePropertyName("status");
    }
}

public sealed partial class EventDefinitionWriteRequestDtoValidator
    : AbstractValidator<EventDefinitionWriteRequestDto>
{
    public EventDefinitionWriteRequestDtoValidator()
    {
        ApplyIdentityRules(this);
    }

    internal static void ApplyIdentityRules<T>(AbstractValidator<T> validator)
        where T : EventDefinitionWriteRequestDto
    {
        validator.RuleFor(request => request.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .MaximumLength(EventDefinitionSchemaLimits.MaximumCodeLength)
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .Must(code => EventCodeRegex().IsMatch(code))
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .OverridePropertyName("code");

        validator.RuleFor(request => request.RoutingKey)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .MaximumLength(EventDefinitionSchemaLimits.MaximumRoutingKeyLength)
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .Must(routingKey => RoutingKeyRegex().IsMatch(routingKey))
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .OverridePropertyName("routingKey");

        validator.RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("NAME_REQUIRED")
            .MaximumLength(EventDefinitionSchemaLimits.MaximumNameLength)
            .WithErrorCode("NAME_TOO_LONG")
            .OverridePropertyName("name");

        validator.RuleFor(request => request.Description)
            .MaximumLength(EventDefinitionSchemaLimits.MaximumDescriptionLength)
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .When(request => request.Description is not null)
            .OverridePropertyName("description");
    }

    [GeneratedRegex("^[A-Z][A-Z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex EventCodeRegex();

    [GeneratedRegex("^[a-z0-9]+(?:\\.[a-z0-9_-]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex RoutingKeyRegex();
}

public sealed class CreateEventDefinitionRequestDtoValidator
    : AbstractValidator<CreateEventDefinitionRequestDto>
{
    public CreateEventDefinitionRequestDtoValidator()
    {
        EventDefinitionWriteRequestDtoValidator.ApplyIdentityRules(this);
        RuleFor(request => request.PayloadSchema)
            .NotNull()
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .OverridePropertyName("payloadSchema");
        RuleFor(request => request.PayloadSchema).Custom(ValidateSchema);
    }

    private static void ValidateSchema(
        EventPayloadSchemaRequestDto? schema,
        ValidationContext<CreateEventDefinitionRequestDto> context)
    {
        EventPayloadSchemaRequestDtoValidator.Validate(schema, context);
    }
}

public sealed class UpdateEventDefinitionDraftRequestDtoValidator
    : AbstractValidator<UpdateEventDefinitionDraftRequestDto>
{
    public UpdateEventDefinitionDraftRequestDtoValidator()
    {
        RuleFor(request => request.PayloadSchema)
            .NotNull()
            .WithErrorCode("EVENT_DEFINITION_SCHEMA_INVALID")
            .OverridePropertyName("payloadSchema");
        RuleFor(request => request.PayloadSchema).Custom(ValidateSchema);
    }

    private static void ValidateSchema(
        EventPayloadSchemaRequestDto? schema,
        ValidationContext<UpdateEventDefinitionDraftRequestDto> context)
    {
        EventPayloadSchemaRequestDtoValidator.Validate(schema, context);
    }
}

internal static class EventPayloadSchemaRequestDtoValidator
{
    public static void Validate<T>(
        EventPayloadSchemaRequestDto? schema,
        ValidationContext<T> context)
    {
        if (schema is null)
        {
            return;
        }

        if (schema.ExtensionData is { Count: > 0 })
        {
            AddFailure(context, "payloadSchema", "EVENT_DEFINITION_SCHEMA_INVALID");
        }

        if (schema.Fields is null)
        {
            AddFailure(context, "payloadSchema.fields", "EVENT_DEFINITION_SCHEMA_INVALID");
        }
        else
        {
            var index = 0;
            foreach (var field in schema.Fields)
            {
                if (field.ExtensionData is { Count: > 0 })
                {
                    AddFailure(context, $"payloadSchema.fields[{index}]", "EVENT_DEFINITION_SCHEMA_INVALID");
                }

                index++;
            }
        }

        if (schema.Targets is null)
        {
            AddFailure(context, "payloadSchema.targets", "EVENT_DEFINITION_SCHEMA_INVALID");
        }
        else
        {
            var index = 0;
            foreach (var target in schema.Targets)
            {
                if (target.ExtensionData is { Count: > 0 })
                {
                    AddFailure(context, $"payloadSchema.targets[{index}]", "EVENT_DEFINITION_SCHEMA_INVALID");
                }

                index++;
            }
        }
    }

    private static void AddFailure<T>(
        ValidationContext<T> context,
        string propertyName,
        string errorCode)
    {
        context.AddFailure(new ValidationFailure(propertyName, "Event definition schema is invalid.")
        {
            ErrorCode = errorCode
        });
    }
}
