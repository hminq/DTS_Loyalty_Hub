namespace Core.UseCases.VoucherDefinitions.Results;

public sealed record VoucherImportTemplateResult(
    string DownloadUrl,
    string FileName,
    DateTimeOffset ExpiresAt);
