using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Campaigns.Handlers;
using Core.UseCases.Campaigns.Queries;
using Core.UseCases.Campaigns.Results;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.Campaigns;

public sealed class GetCampaignByIdQueryHandlerTests
{
    private readonly Mock<ICampaignRepository> _repository = new();
    private readonly Mock<IBannerReadUrlProvider> _bannerReadUrlProvider = new();

    [Fact]
    public async Task Handle_CampaignHasBanner_ReturnsKeyAndGeneratedReadUrl()
    {
        var campaignId = Guid.NewGuid();
        const string bannerKey = "campaigns/banners/banner.webp";
        const string readUrl = "https://s3.example.test/signed-banner";
        _repository.Setup(repository => repository.GetByIdAsync(
                campaignId,
                100,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Campaign(campaignId, bannerKey));
        _bannerReadUrlProvider.Setup(provider => provider.CreateReadUrl(bannerKey))
            .Returns(readUrl);
        var handler = new GetCampaignByIdQueryHandler(
            _repository.Object,
            _bannerReadUrlProvider.Object);

        var result = await handler.Handle(
            new GetCampaignByIdQuery(campaignId),
            CancellationToken.None);

        result.BannerImageKey.Should().Be(bannerKey);
        result.BannerImageUrl.Should().Be(readUrl);
    }

    [Fact]
    public async Task Handle_CampaignHasNoBanner_DoesNotGenerateReadUrl()
    {
        var campaignId = Guid.NewGuid();
        _repository.Setup(repository => repository.GetByIdAsync(
                campaignId,
                100,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Campaign(campaignId, null));
        var handler = new GetCampaignByIdQueryHandler(
            _repository.Object,
            _bannerReadUrlProvider.Object);

        var result = await handler.Handle(
            new GetCampaignByIdQuery(campaignId),
            CancellationToken.None);

        result.BannerImageKey.Should().BeNull();
        result.BannerImageUrl.Should().BeNull();
        _bannerReadUrlProvider.Verify(
            provider => provider.CreateReadUrl(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CampaignDoesNotExist_ThrowsNotFound()
    {
        var campaignId = Guid.NewGuid();
        _repository.Setup(repository => repository.GetByIdAsync(
                campaignId,
                100,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CampaignDetailResult?)null);
        var handler = new GetCampaignByIdQueryHandler(
            _repository.Object,
            _bannerReadUrlProvider.Object);

        var act = () => handler.Handle(
            new GetCampaignByIdQuery(campaignId),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_NOT_FOUND");
    }

    private static CampaignDetailResult Campaign(Guid campaignId, string? bannerKey)
    {
        var now = new DateTime(2026, 7, 27, 10, 0, 0, DateTimeKind.Utc);
        return new CampaignDetailResult(
            campaignId,
            "Registration reward",
            null,
            bannerKey,
            null,
            "CUSTOMER_ACCOUNT_REGISTERED",
            now.AddDays(1),
            now.AddDays(31),
            """{"all":[]}""",
            "0 0 2 * * ?",
            2,
            1,
            1,
            "DRAFT",
            now,
            now,
            [],
            [],
            0);
    }
}
