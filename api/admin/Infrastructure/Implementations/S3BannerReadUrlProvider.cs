using Amazon.S3;
using Amazon.S3.Model;
using Core.Abstractions;
using Infrastructure.Options;

namespace Infrastructure.Implementations;

public sealed class S3BannerReadUrlProvider : IBannerReadUrlProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;

    public S3BannerReadUrlProvider(IAmazonS3 s3Client, S3Options options)
    {
        _s3Client = s3Client;
        _options = options;
    }

    public string CreateReadUrl(string objectKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

        return _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey.Trim(),
            Expires = DateTime.UtcNow.AddMinutes(_options.PresignedUrlExpirationMinutes),
            Protocol = Protocol.HTTPS,
            Verb = HttpVerb.GET
        });
    }
}
