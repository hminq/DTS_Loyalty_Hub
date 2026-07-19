using System.Globalization;
using Api.Localization;
using Api.Mappers;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Mappers;

[Collection(LocalizationTestCollection.Name)]
public sealed class ValidationErrorMapperTests
{
    [Fact]
    public void FromValidationFailures_UsesVietnameseResourceMessage()
    {
        using var cultureScope = new CultureScope("vi");
        var mapper = CreateMapper();
        var failure = new ValidationFailure("pageSize", "Legacy message")
        {
            ErrorCode = "PAGE_SIZE_INVALID"
        };

        var response = mapper.FromValidationFailures([failure]);

        response.Error.Message.Should().Be("Dữ liệu yêu cầu không hợp lệ.");
        response.Error.Details.Should().ContainSingle().Which.Message
            .Should().Be("Kích thước trang phải từ 1 đến 100.");
    }

    [Fact]
    public void FromModelState_DoesNotExposeFrameworkMessage()
    {
        using var cultureScope = new CultureScope("vi");
        var mapper = CreateMapper();
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("AdminId", "Raw framework parsing message");

        var response = mapper.FromModelState(modelState);

        var error = response.Error.Details.Should().ContainSingle().Which;
        error.Field.Should().Be("adminId");
        error.Code.Should().Be("VALUE_INVALID");
        error.Message.Should().Be("Giá trị trong yêu cầu không hợp lệ.");
    }

    private static ValidationErrorMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddScoped<ApiMessageResolver>();
        services.AddScoped<ValidationErrorMapper>();
        return services.BuildServiceProvider().GetRequiredService<ValidationErrorMapper>();
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _previousUiCulture = CultureInfo.CurrentUICulture;

        public CultureScope(string cultureName)
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            CultureInfo.CurrentUICulture = _previousUiCulture;
        }
    }
}
