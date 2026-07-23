using Amazon.S3;
using Amazon.S3.Model;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Infrastructure.Options;

namespace Infrastructure.Implementations;

public sealed class S3BannerStorage : IBannerStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;

    public S3BannerStorage(IAmazonS3 s3Client, S3Options options)
    {
        _s3Client = s3Client;
        _options = options;
    }

    public async Task<BannerUploadResult> UploadAsync(
        Stream content,
        string contentType,
        string uploadType,
        CancellationToken ct)
    {
        var extension = BannerFileTypes.GetExtension(contentType);

        var prefix = uploadType switch
        {
            BannerUploadTypes.CampaignBanner => BannerUploadTypes.CampaignBannerPrefix,
            BannerUploadTypes.VoucherDefinitionBanner => BannerUploadTypes.VoucherDefinitionBannerPrefix,
            _ => throw new DomainException(
                "BANNER_UPLOAD_TYPE_INVALID",
                DomainErrorType.Validation)
        };

        var key = $"{prefix}{Guid.NewGuid():N}{extension}";
        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType
        }, ct);

        return new BannerUploadResult(key);
    }

}
