using Microsoft.EntityFrameworkCore;

namespace Platform.Application.Common.Pagination;

public sealed class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public long TotalCount { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedList(IReadOnlyList<T> items, long count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 10 : pageSize;
        TotalCount = count;
        TotalPages = (int)Math.Ceiling(count / (double)PageSize);
        Items = items;
    }

    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var page = pageNumber < 1 ? 1 : pageNumber;
        var size = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);
        var count = await source.LongCountAsync(cancellationToken);
        var items = await source.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return new PagedList<T>(items, count, page, size);
    }
}

public record PagedRequest(int PageNumber = 1, int PageSize = 10);

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalPages,
    long TotalCount,
    bool HasPreviousPage,
    bool HasNextPage);
