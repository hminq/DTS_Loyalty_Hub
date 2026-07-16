public interface IUnitOfWork
{
    Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}