using Core.Entities.Constants;
using Core.Exceptions;
using FluentAssertions;

namespace Core.Tests.Entities;

public sealed class BannerFileTypesTests
{
    [Theory]
    [InlineData(BannerFileTypes.Jpeg, ".jpg")]
    [InlineData(BannerFileTypes.Png, ".png")]
    [InlineData(BannerFileTypes.WebP, ".webp")]
    [InlineData("IMAGE/WEBP", ".webp")]
    public void GetExtension_SupportedContentType_ReturnsExpectedExtension(
        string contentType,
        string expectedExtension)
    {
        BannerFileTypes.GetExtension(contentType).Should().Be(expectedExtension);
    }

    [Fact]
    public void GetExtension_UnsupportedContentType_ThrowsDomainException()
    {
        var action = () => BannerFileTypes.GetExtension("image/svg+xml");

        action.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("BANNER_FILE_TYPE_INVALID");
    }
}
