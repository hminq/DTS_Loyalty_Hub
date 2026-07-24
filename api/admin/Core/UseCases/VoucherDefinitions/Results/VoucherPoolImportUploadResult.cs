namespace Core.UseCases.VoucherDefinitions.Results;

public sealed record VoucherPoolImportUploadResult(
    string ObjectKey,
    string UploadUrl,
    string Method,
    string ContentType,
    DateTimeOffset ExpiresAt,
    long MaximumFileSizeBytes);
