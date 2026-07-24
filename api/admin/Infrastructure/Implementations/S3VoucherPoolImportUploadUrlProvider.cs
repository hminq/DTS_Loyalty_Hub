using Amazon.S3;
using Amazon.S3.Model;
using Core.Abstractions;
using Infrastructure.Options;

namespace Infrastructure.Implementations;

public sealed class S3VoucherPoolImportUploadUrlProvider :
    IVoucherPoolImportUploadUrlProvider,
    IVoucherPoolImportObjectKeyPolicy
{
    private const string ContentType = "text/csv";

    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _options;
    private readonly TimeProvider _timeProvider;

    public S3VoucherPoolImportUploadUrlProvider(
        IAmazonS3 s3Client,
        S3Options options,
        TimeProvider timeProvider)
    {
        _s3Client = s3Client;
        _options = options;
        _timeProvider = timeProvider;
    }

    public long MaximumFileSizeBytes =>
        _options.VoucherPoolImportMaximumFileSizeBytes;

    public VoucherPoolImportUpload CreateUpload(Guid voucherDefinitionId)
    {
        var objectKey =
            $"voucher_defs/{voucherDefinitionId:D}/imports/{Guid.NewGuid():D}.csv";
        var expiresAt = _timeProvider.GetUtcNow()
            .AddMinutes(_options.PresignedUrlExpirationMinutes);
        var uploadUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Protocol = Protocol.HTTPS,
            ContentType = ContentType,
            Expires = expiresAt.UtcDateTime
        });

        return new VoucherPoolImportUpload(
            objectKey,
            uploadUrl,
            "PUT",
            ContentType,
            expiresAt);
    }

    public bool IsValid(Guid voucherDefinitionId, string? objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return false;
        }

        var prefix = $"voucher_defs/{voucherDefinitionId:D}/imports/";
        if (!objectKey.StartsWith(prefix, StringComparison.Ordinal) ||
            !objectKey.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var fileName = objectKey[prefix.Length..^4];
        return Guid.TryParseExact(fileName, "D", out _);
    }
}
