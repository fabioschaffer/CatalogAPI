using System.Text.Json;
using Domain.Interfaces;
using System.Net.Http.Json;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Search;

public sealed class ElasticsearchGameIndex(HttpClient client, IOptions<ElasticsearchSettings> options)
    : IGameSearchIndex
{
    public async Task<IReadOnlyList<GameSearchResult>> SearchAsync(string query, int size = 20,
        CancellationToken cancellationToken = default)
    {
        var index = Uri.EscapeDataString(options.Value.IndexName);
        using var response = await client.PostAsJsonAsync($"{index}/_search", new
        {
            size,
            query = new { match = new { nome = new { query, fuzziness = "AUTO" } } },
            sort = new[] { new { _score = "desc" } }
        }, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray()
            .Select(hit =>
            {
                var source = hit.GetProperty("_source");
                return new GameSearchResult(
                    source.GetProperty("id").GetInt32(),
                    source.GetProperty("nome").GetString() ?? string.Empty,
                    source.GetProperty("price").GetDouble(),
                    hit.GetProperty("_score").GetDouble());
            }).ToList();
    }

    public async Task IndexAsync(Game game, CancellationToken cancellationToken)
    {
        var index = Uri.EscapeDataString(options.Value.IndexName);
        // A stable document ID makes edits overwrite the existing document.
        using var response = await client.PutAsJsonAsync(
            $"{index}/_doc/{game.Id}?refresh=wait_for",
            new { id = game.Id, nome = game.Nome, price = game.Price }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
