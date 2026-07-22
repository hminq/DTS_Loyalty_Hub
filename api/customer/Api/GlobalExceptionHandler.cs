using Api.Dtos.Responses;
using Api.Localization;
using Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Api;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        // Resolve the scoped ApiMessageResolver per-request from httpContext
        var messageResolver = httpContext.RequestServices.GetRequiredService<ApiMessageResolver>();

        var (statusCode, errorResponse) = exception switch
        {
            DomainException domainException => CreateDomainError(domainException, messageResolver),
            _ => CreateUnknownError(messageResolver)
        };

        if (exception is DomainException handledDomainException)
        {
            logger.LogWarning(
                "Handled domain exception {ErrorCode} with HTTP {StatusCode} for {RequestMethod} {RequestPath}. TraceId: {TraceId}",
                handledDomainException.ErrorCode,
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogError(
                exception,
                "Unhandled exception for {RequestMethod} {RequestPath}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(errorResponse, ct);

        return true;
    }

    private (int StatusCode, ApiErrorResponseDto ErrorResponse) CreateDomainError(
        DomainException exception, 
        ApiMessageResolver messageResolver)
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

        return (statusCode, ApiErrorResponseDto.Create(
            exception.ErrorCode, 
            messageResolver.Resolve(exception.ErrorCode, exception.MessageArguments)));
    }

    private (int StatusCode, ApiErrorResponseDto ErrorResponse) CreateUnknownError(
        ApiMessageResolver messageResolver)
    {
        return (
            StatusCodes.Status500InternalServerError,
            ApiErrorResponseDto.Create(
                "INTERNAL_SERVER_ERROR",
                messageResolver.Resolve("INTERNAL_SERVER_ERROR")));
    }
}