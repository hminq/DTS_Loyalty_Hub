namespace Api.Dtos.Responses.VoucherDefinitions;

/// <summary>
/// Represents a short-lived download for the voucher-code CSV import template.
/// </summary>
public sealed record VoucherImportTemplateResponseDto(
    string DownloadUrl,
    string FileName,
    DateTimeOffset ExpiresAt);
