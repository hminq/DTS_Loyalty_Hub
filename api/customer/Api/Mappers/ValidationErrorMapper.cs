using Api.Dtos.Responses;
using Api.Localization;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Mappers;

public sealed class ValidationErrorMapper(ApiMessageResolver messageResolver)
{
    public ApiErrorResponseDto FromModelState(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
            {
                var field = NormalizeField(entry.Key);
                var errorCode = field == "request"
                    ? "REQUEST_BODY_INVALID"
                    : "VALUE_INVALID";

                return new ApiValidationErrorDto
                {
                    Field = field,
                    Code = errorCode,
                    Message = messageResolver.Resolve(errorCode)
                };
            }))
            .ToArray();

        return ApiErrorResponseDto.Validation(
            errors,
            messageResolver.Resolve("VALIDATION_ERROR"));
    }

    public ApiErrorResponseDto FromValidationFailures(
        IEnumerable<ValidationFailure> validationFailures)
    {
        var errors = validationFailures
            .Select(error => new ApiValidationErrorDto
            {
                Field = NormalizeField(error.PropertyName),
                Code = error.ErrorCode,
                Message = messageResolver.Resolve(
                    error.ErrorCode,
                    error.FormattedMessagePlaceholderValues)
            })
            .ToArray();

        return ApiErrorResponseDto.Validation(
            errors,
            messageResolver.Resolve("VALIDATION_ERROR"));
    }

    private static string NormalizeField(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return "request";
        }

        var normalizedField = field.Trim();

        if (normalizedField.StartsWith("$.", StringComparison.Ordinal))
        {
            normalizedField = normalizedField[2..];
        }

        return char.ToLowerInvariant(normalizedField[0]) + normalizedField[1..];
    }
}
