namespace Core.Abstractions;

public interface IVoucherPoolImportUploadUrlProvider
{
    long MaximumFileSizeBytes { get; }

    VoucherPoolImportUpload CreateUpload(Guid voucherDefinitionId);
}

public sealed record VoucherPoolImportUpload(
    string ObjectKey,
    string UploadUrl,
    string Method,
    string ContentType,
    DateTimeOffset ExpiresAt);
