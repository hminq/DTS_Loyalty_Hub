using Api.Dtos.Requests.Uploads;
using Api.Validators.Uploads;
using Core.Entities.Constants;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Validators.Uploads;

public sealed class UploadBannerRequestDtoValidatorTests
{
    private readonly UploadBannerRequestDtoValidator _sut = new();

    [Theory]
    [InlineData(BannerUploadTypes.CampaignBanner)]
    [InlineData(BannerUploadTypes.VoucherDefinitionBanner)]
    public void Validate_AllowedTypeAndImage_HasNoErrors(string type)
    {
        var request = new UploadBannerRequestDto
        {
            Type = type,
            File = CreateFile(1024, BannerFileTypes.WebP)
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_UnknownType_ReturnsTypeError()
    {
        var request = new UploadBannerRequestDto
        {
            Type = "OTHER",
            File = CreateFile(1024, BannerFileTypes.Png)
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("type")
            .WithErrorCode("BANNER_UPLOAD_TYPE_INVALID");
    }

    [Fact]
    public void Validate_MissingFile_ReturnsRequiredError()
    {
        var result = _sut.TestValidate(new UploadBannerRequestDto
        {
            Type = BannerUploadTypes.CampaignBanner
        });

        result.ShouldHaveValidationErrorFor("file")
            .WithErrorCode("BANNER_FILE_REQUIRED");
    }

    [Theory]
    [InlineData(BannerUploadTypes.CampaignBanner, 0)]
    [InlineData(BannerUploadTypes.CampaignBanner, BannerUploadTypes.CampaignBannerMaxFileSize + 1)]
    [InlineData(BannerUploadTypes.VoucherDefinitionBanner, BannerUploadTypes.VoucherDefinitionBannerMaxFileSize + 1)]
    public void Validate_InvalidSizeForType_ReturnsSizeError(string type, long length)
    {
        var request = new UploadBannerRequestDto
        {
            Type = type,
            File = CreateFile(length, BannerFileTypes.Png)
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("file")
            .WithErrorCode("BANNER_FILE_SIZE_INVALID");
    }

    [Fact]
    public void Validate_UnsupportedMimeType_ReturnsFileTypeError()
    {
        var request = new UploadBannerRequestDto
        {
            Type = BannerUploadTypes.CampaignBanner,
            File = CreateFile(1024, "image/svg+xml")
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor("file")
            .WithErrorCode("BANNER_FILE_TYPE_INVALID");
    }

    private static IFormFile CreateFile(long length, string contentType)
    {
        return new FormFile(Stream.Null, 0, length, "file", "banner")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
