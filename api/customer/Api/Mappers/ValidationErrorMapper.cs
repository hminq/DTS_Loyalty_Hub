using Api.Dtos.Responses;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Mappers;

public static class ValidationErrorMapper
{
    public static IReadOnlyCollection<ApiValidationErrorDto> FromModelState(ModelStateDictionary modelState)
    {
        return modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new ApiValidationErrorDto
            {
                Field = NormalizeField(entry.Key),
                Code = "INVALID_REQUEST_VALUE",
                Message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Request value is invalid."
                    : error.ErrorMessage
            }))
            .ToArray();
    }

    public static IReadOnlyCollection<ApiValidationErrorDto> FromValidationFailures(
        IEnumerable<ValidationFailure> validationFailures)
    {
        return validationFailures
            .Select(error => new ApiValidationErrorDto
            {
                Field = NormalizeField(error.PropertyName),
                Code = error.ErrorCode,
                Message = error.ErrorMessage
            })
            .ToArray();
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