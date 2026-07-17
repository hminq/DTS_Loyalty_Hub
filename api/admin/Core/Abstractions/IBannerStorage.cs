namespace Core.Abstractions;

public interface IBannerStorage
{
    Task<BannerUploadResult> UploadAsync(
        Stream content,
        string contentType,
        string uploadType,
        CancellationToken ct);
}

public sealed record BannerUploadResult(string Key, string Url);
