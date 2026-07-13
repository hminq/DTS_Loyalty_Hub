using Api.Dtos.Responses;
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

    private static string NormalizeField(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return "request";
        }

        return char.ToLowerInvariant(field[0]) + field[1..];
    }
}
