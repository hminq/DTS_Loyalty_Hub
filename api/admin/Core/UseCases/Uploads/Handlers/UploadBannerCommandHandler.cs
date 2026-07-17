using Core.Abstractions;
using Core.UseCases.Uploads.Commands;
using MediatR;

namespace Core.UseCases.Uploads.Handlers;

public sealed class UploadBannerCommandHandler
    : IRequestHandler<UploadBannerCommand, UploadBannerResult>
{
    private readonly IBannerStorage _bannerStorage;

    public UploadBannerCommandHandler(IBannerStorage bannerStorage)
    {
        _bannerStorage = bannerStorage;
    }

    public async Task<UploadBannerResult> Handle(
        UploadBannerCommand request,
        CancellationToken ct)
    {
        var uploaded = await _bannerStorage.UploadAsync(
            request.Content,
            request.ContentType,
            request.UploadType,
            ct);

        return new UploadBannerResult(uploaded.Key, uploaded.Url);
    }
}
