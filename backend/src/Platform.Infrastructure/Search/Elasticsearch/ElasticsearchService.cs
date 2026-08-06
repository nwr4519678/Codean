using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Common.Contracts.Search;
using Platform.Domain.Results;

namespace Platform.Infrastructure.Search.Elasticsearch;

public sealed class ElasticsearchOptions
{
    public string Url { get; set; } = "";
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public sealed class ElasticsearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService> _logger;

    public ElasticsearchService(IOptions<ElasticsearchOptions> opt, ILogger<ElasticsearchService> logger)
    {
        var settings = new ElasticsearchClientSettings(new Uri(opt.Value.Url))
            .DefaultIndex("platform");
        if (!string.IsNullOrEmpty(opt.Value.Username) && !string.IsNullOrEmpty(opt.Value.Password))
        {
            settings = settings.Authentication(new BasicAuthentication(opt.Value.Username!, opt.Value.Password!));
        }
        _client = new ElasticsearchClient(settings);
        _logger = logger;
    }

    public async Task IndexAsync(string index, string id, object document, CancellationToken ct = default)
    {
        try { await _client.IndexAsync(document, i => i.Index(index).Id(id), ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "ES index failed for {Id}", id); }
    }

    public async Task BulkIndexAsync(string index, IEnumerable<(string Id, object Doc)> docs, CancellationToken ct = default)
    {
        try
        {
            var docsList = docs.ToList();
            var bulk = await _client.BulkAsync(b =>
            {
                b.Index(index);
                foreach (var d in docsList)
                    b.Index(d.Doc, op => op.Id(d.Id));
            }, ct);
            if (bulk.Errors) _logger.LogWarning("ES bulk had errors");
        }
        catch (Exception ex) { _logger.LogWarning(ex, "ES bulk failed"); }
    }

    public async Task RemoveAsync(string index, string id, CancellationToken ct = default)
    {
        try { await _client.DeleteAsync<JsonObject>(id, idx => idx.Index(index), ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "ES delete failed for {Id}", id); }
    }

    public async Task<SearchResults> SearchAsync(string index, string query, int from = 0, int size = 20, CancellationToken ct = default)
    {
        try
        {
            var resp = await _client.SearchAsync<JsonObject>(s => s
                .Index(index)
                .From(from)
                .Size(size)
                .Query(q => q.MultiMatch(mm => mm
                    .Query(query)
                    .Fields(new[] { "title^3", "description", "tags" })
                    .Fuzziness(new Fuzziness("AUTO")))),
                ct);

            var hits = resp.Hits.Select(h => new SearchHit(
                h.Id ?? "",
                h.Source?["title"]?.ToString() ?? "",
                h.Source?["description"]?.ToString(),
                h.Source?.ToDictionary(kv => kv.Key, kv => (object?)kv.Value) ?? new()
            )).ToList();
            return new SearchResults(resp.Total, hits);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ES search failed");
            return new SearchResults(0, new List<SearchHit>());
        }
    }
}

internal sealed class JsonObject : Dictionary<string, object?> { }
