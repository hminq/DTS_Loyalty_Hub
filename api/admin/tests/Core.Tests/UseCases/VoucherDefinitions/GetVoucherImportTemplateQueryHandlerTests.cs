using Core.Abstractions;
using Core.UseCases.VoucherDefinitions.Handlers;
using Core.UseCases.VoucherDefinitions.Queries;
using FluentAssertions;
using Moq;

namespace Core.Tests.UseCases.VoucherDefinitions;

public sealed class GetVoucherImportTemplateQueryHandlerTests
{
    [Fact]
    public async Task Handle_MapsProviderDownload()
    {
        var expiresAt = new DateTimeOffset(
            2026,
            7,
            24,
            10,
            15,
            0,
            TimeSpan.Zero);
        var provider = new Mock<IVoucherImportTemplateUrlProvider>();
        provider.Setup(item => item.CreateDownload())
            .Returns(new VoucherImportTemplateDownload(
                "https://example.test/template.csv",
                "import-code-template.csv",
                expiresAt));
        var handler = new GetVoucherImportTemplateQueryHandler(provider.Object);

        var result = await handler.Handle(
            new GetVoucherImportTemplateQuery(),
            CancellationToken.None);

        result.DownloadUrl.Should().Be("https://example.test/template.csv");
        result.FileName.Should().Be("import-code-template.csv");
        result.ExpiresAt.Should().Be(expiresAt);
    }
}
