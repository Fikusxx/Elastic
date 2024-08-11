using Elastic.Clients.Elasticsearch;

namespace Elastic.Common;

public static class ElasticConstants
{
    public const string IndexName = "my_index";
    public static readonly Guid Id = Guid.Parse("82157646-b752-4899-904d-562dfd02f20c");

    public static ElasticsearchClient GetClient()
    {
        var settings = new ElasticsearchClientSettings();
        settings.DefaultIndex(ElasticConstants.IndexName)
            .DefaultMappingFor<Game>(x =>
            {
                x.IndexName(ElasticConstants.IndexName);
                x.IdProperty(game => game.Id);
            })
            .EnableDebugMode()
            .PrettyJson()
            .RequestTimeout(TimeSpan.FromSeconds(2))
            .MaximumRetries(3);

        var client = new ElasticsearchClient(settings);
        return client;
    }
}