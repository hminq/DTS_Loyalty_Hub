using Amazon.S3;
using Amazon.S3.Model;
using Core.Abstractions;
using Infrastructure.Options;

namespace Infrastructure.Implementations;

public sealed class S3VoucherImportTemplateUrlProvider
    : IVoucherImportTemplateUrlProvider
{
    private const string DownloadFileName = "import-code-template.csv";

    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;
    private readonly TimeProvider _timeProvider;

    public S3VoucherImportTemplateUrlProvider(
        IAmazonS3 s3Client,
        S3Options options,
        TimeProvider timeProvider)
    {
        _s3Client = s3Client;
        _options = options;
        _timeProvider = timeProvider;
    }

    public VoucherImportTemplateDownload CreateDownload()
    {
        var expiresAt = _timeProvider
            .GetUtcNow()
            .AddMinutes(_options.PresignedUrlExpirationMinutes);

        var downloadUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = _options.ImportTemplateFileKey,
            Expires = expiresAt.UtcDateTime,
            Protocol = Protocol.HTTPS,
            Verb = HttpVerb.GET,
            ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentDisposition =
                    $"attachment; filename=\"{DownloadFileName}\"",
                ContentType = "text/csv"
            }
        });

        return new VoucherImportTemplateDownload(
            downloadUrl,
            DownloadFileName,
            expiresAt);
    }
}
