using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Common;
using Elastic.Transport;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("utility")]
public sealed class Utility : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();

    [HttpGet]
    [Route("ping")]
    public async Task<IActionResult> Ping()
    {
        var response = await client.PingAsync();

        return Ok(response);
    }

    /// <summary>
    /// Each shard is an index. Shard may contain up to 2B records
    /// </summary>
    [HttpPost]
    [Route("create-index")]
    public async Task<IActionResult> CreateIndex()
    {
        // var response = await client.Indices.CreateAsync(ElasticConstants.IndexName);
        var response = await client.Indices.CreateAsync<Game>(ElasticConstants.IndexName, x =>
        {
            x.Mappings(m => m
                    .Properties(p => p
                        .Keyword(n => n.Id) // keyword means we know exactly what value would be, i.e id
                        .Text(n => n.Title)
                        .IntegerNumber(n => n.Price)))
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0) // requires >1 nodes, default 1
                    
                    // default 60s. Old optimistic concurrency control mechanism.
                    // Defines for how long _version value is kept when document is deleted.
                    // For scenarios when a new document with the same id is created right away
                    .GcDeletes(TimeSpan.FromSeconds(60))); 
                                                            
        });

        return Ok(response);
    }

    [HttpDelete]
    [Route("delete-index")]
    public async Task<IActionResult> DeleteIndex()
    {
        var response = await client.Indices.DeleteAsync(ElasticConstants.IndexName);

        return Ok(response);
    }
}