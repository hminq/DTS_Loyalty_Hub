using Core.Abstractions;
using Core.Entities.Constants;
using Core.UseCases.Uploads.Commands;
using Core.UseCases.Uploads.Handlers;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Uploads;

public sealed class UploadBannerCommandHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesUploadAndReturnsStorageResult()
    {
        var content = new MemoryStream([1, 2, 3]);
        var storage = new Mock<IBannerStorage>();
        storage.Setup(x => x.UploadAsync(
                content,
                BannerFileTypes.Png,
                BannerUploadTypes.CampaignBanner,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BannerUploadResult("campaigns/banners/id.png"));

        var handler = new UploadBannerCommandHandler(storage.Object);
        var result = await handler.Handle(new UploadBannerCommand(
            content,
            BannerFileTypes.Png,
            BannerUploadTypes.CampaignBanner), CancellationToken.None);

        result.Key.Should().Be("campaigns/banners/id.png");
        storage.VerifyAll();
    }
}
