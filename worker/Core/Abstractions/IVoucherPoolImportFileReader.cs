using Core.Entities;

namespace Core.Abstractions;

public interface IVoucherPoolImportFileReader
{
    IAsyncEnumerable<VoucherPoolImportRawRow> ReadAsync(
        string objectKey,
        CancellationToken cancellationToken);
}
