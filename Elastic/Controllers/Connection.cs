using Elastic.Clients.Elasticsearch;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

/// <summary>
/// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/_options_on_elasticsearchclientsettings.html
/// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/recommendations.html#_reuse_the_same_client_instance
/// </summary>
[ApiController]
[Route("connection")]
public sealed class Connection : ControllerBase
{
    // should be singleton / static
    private readonly ElasticsearchClient client;

    public Connection()
    {
        var settings = new ElasticsearchClientSettings(); // defaults to http://localhost:9200
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

        this.client = new ElasticsearchClient(settings);
    }
}