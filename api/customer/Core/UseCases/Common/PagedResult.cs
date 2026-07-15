namespace Core.UseCases.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; }
    public int TotalCount { get; }

    public PagedResult(IEnumerable<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}
