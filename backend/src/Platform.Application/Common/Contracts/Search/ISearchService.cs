namespace Platform.Application.Common.Contracts.Search;

public sealed record SearchHit(string Id, string Title, string? Description, IReadOnlyDictionary<string, object?> Data);
public sealed record SearchResults(long Total, IReadOnlyList<SearchHit> Hits);

public interface ISearchService
{
    Task IndexAsync(string index, string id, object document, CancellationToken ct = default);
    Task BulkIndexAsync(string index, IEnumerable<(string Id, object Doc)> docs, CancellationToken ct = default);
    Task RemoveAsync(string index, string id, CancellationToken ct = default);
    Task<SearchResults> SearchAsync(string index, string query, int from = 0, int size = 20, CancellationToken ct = default);
}
