using MediatR;

namespace Core.UseCases.Uploads.Commands;

public sealed record UploadBannerCommand(
    Stream Content,
    string ContentType,
    string UploadType) : IRequest<UploadBannerResult>;
