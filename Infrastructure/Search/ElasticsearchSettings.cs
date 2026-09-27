namespace Infrastructure.Search;

public sealed class ElasticsearchSettings
{
    public const string SectionName = "Elasticsearch";
    public string Url { get; set; } = "https://my-elasticsearc";
    public string IndexName { get; set; } = "catalog-games";
    public string? ApiKey { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

}
