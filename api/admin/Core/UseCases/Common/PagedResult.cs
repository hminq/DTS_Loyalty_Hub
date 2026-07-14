namespace Core.UseCases.Common;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling((double)TotalItems / PageSize);
}
