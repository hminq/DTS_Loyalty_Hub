namespace Core.Abstractions;

public interface IVoucherImportTemplateUrlProvider
{
    VoucherImportTemplateDownload CreateDownload();
}

public sealed record VoucherImportTemplateDownload(
    string DownloadUrl,
    string FileName,
    DateTimeOffset ExpiresAt);
