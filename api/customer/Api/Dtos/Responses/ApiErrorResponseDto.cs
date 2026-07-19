namespace Api.Dtos.Responses;

public sealed class ApiErrorResponseDto
{
    public ApiErrorDto Error { get; set; } = null!;

    public static ApiErrorResponseDto Create(string code, string message)
    {
        return new ApiErrorResponseDto
        {
            Error = new ApiErrorDto
            {
                Code = code,
                Message = message
            }
        };
    }

    public static ApiErrorResponseDto Validation(
        IReadOnlyCollection<ApiValidationErrorDto> details,
        string message)
    {
        return new ApiErrorResponseDto
        {
            Error = new ApiErrorDto
            {
                Code = "VALIDATION_ERROR",
                Message = message,
                Details = details
            }
        };
    }
}

public sealed class ApiErrorDto
{
    public string Code { get; set; } = null!;

    public string Message { get; set; } = null!;

    public IReadOnlyCollection<ApiValidationErrorDto>? Details { get; set; }
}

public sealed class ApiValidationErrorDto
{
    public string Field { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Message { get; set; } = null!;
}
