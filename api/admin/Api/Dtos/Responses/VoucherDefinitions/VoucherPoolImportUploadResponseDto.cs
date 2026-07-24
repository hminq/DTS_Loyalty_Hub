namespace Api.Dtos.Responses.VoucherDefinitions;

/// <summary>Contains the signed S3 upload capability and server-owned object key.</summary>
public sealed record VoucherPoolImportUploadResponseDto(
    string ObjectKey,
    string UploadUrl,
    string Method,
    string ContentType,
    DateTimeOffset ExpiresAt,
    long MaximumFileSizeBytes);
