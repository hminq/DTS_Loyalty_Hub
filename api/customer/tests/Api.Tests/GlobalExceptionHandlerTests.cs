using System.Globalization;
using System.Text.Json;
using Api;
using Api.Localization;
using Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Tests;

[Collection(LocalizationTestCollection.Name)]
public sealed class GlobalExceptionHandlerTests
{
    [Theory]
    [InlineData("en", "Username or password is incorrect.")]
    [InlineData("vi", "Tên đăng nhập hoặc mật khẩu không chính xác.")]
    public async Task TryHandleAsync_LocalizesDomainErrorForCurrentCulture(
        string cultureName,
        string expectedMessage)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
            var handler = CreateHandler();
            var context = CreateHttpContext();
            var exception = new DomainException(
                "INVALID_CREDENTIALS",
                DomainErrorType.Unauthorized);

            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            handled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            (await ReadErrorAsync(context)).Should().Be(("INVALID_CREDENTIALS", expectedMessage));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static GlobalExceptionHandler CreateHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddScoped<ApiMessageResolver>();
        var provider = services.BuildServiceProvider();

        return new GlobalExceptionHandler(
            provider.GetRequiredService<ILogger<GlobalExceptionHandler>>(),
            provider.GetRequiredService<ApiMessageResolver>());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<(string Code, string Message)> ReadErrorAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var error = document.RootElement.GetProperty("error");
        return (
            error.GetProperty("code").GetString()!,
            error.GetProperty("message").GetString()!);
    }
}
