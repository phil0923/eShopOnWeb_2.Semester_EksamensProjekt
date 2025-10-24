namespace ProductCatalog.Application.DTOs;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalItems { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
}