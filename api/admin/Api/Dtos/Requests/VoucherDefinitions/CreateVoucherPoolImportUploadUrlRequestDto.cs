namespace Api.Dtos.Requests.VoucherDefinitions;

/// <summary>Requests a server-scoped S3 upload URL for a voucher-code CSV.</summary>
public sealed record CreateVoucherPoolImportUploadUrlRequestDto(
    string FileName,
    long FileSizeBytes);
