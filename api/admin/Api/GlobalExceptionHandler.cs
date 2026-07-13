using Api.Dtos.Responses;
using Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Api;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (statusCode, errorResponse) = exception switch
        {
            DomainException domainException => CreateDomainError(domainException),
            _ => CreateUnknownError()
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(errorResponse, ct);

        return true;
    }

    private static (int StatusCode, ApiErrorResponseDto ErrorResponse) CreateDomainError(DomainException exception)
    {
        var statusCode = exception.ErrorType switch
        {
            DomainErrorType.Validation => StatusCodes.Status400BadRequest,
            DomainErrorType.Conflict => StatusCodes.Status409Conflict,
            DomainErrorType.NotFound => StatusCodes.Status404NotFound,
            DomainErrorType.Forbidden => StatusCodes.Status403Forbidden,
            DomainErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        return (statusCode, ApiErrorResponseDto.Create(exception.ErrorCode, exception.Message));
    }

    private static (int StatusCode, ApiErrorResponseDto ErrorResponse) CreateUnknownError()
    {
        return (
            StatusCodes.Status500InternalServerError,
            ApiErrorResponseDto.Create("INTERNAL_SERVER_ERROR", "An unexpected error occurred."));
    }
}
