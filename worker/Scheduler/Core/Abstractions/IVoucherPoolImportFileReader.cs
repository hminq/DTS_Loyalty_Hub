using Scheduler.Core.Entities;

namespace Scheduler.Core.Abstractions;

public interface IVoucherPoolImportFileReader
{
    IAsyncEnumerable<VoucherPoolImportRawRow> ReadAsync(
        string objectKey,
        CancellationToken cancellationToken);
}
