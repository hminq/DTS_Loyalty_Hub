using Microsoft.AspNetCore.Http;

namespace Api.Dtos.Requests.Uploads;

public sealed record UploadBannerRequestDto
{
    public string Type { get; init; } = string.Empty;

    public IFormFile? File { get; init; }
}
