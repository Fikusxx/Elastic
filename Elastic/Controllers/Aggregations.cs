using Elastic.Clients.Elasticsearch;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("aggregations")]
public class Aggregations : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();
    
    [HttpGet]
    [Route("a")]
    public async Task<IActionResult> a()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName));


        return Ok();
    }
    
    [HttpGet]
    [Route("b")]
    public async Task<IActionResult> b()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName));


        return Ok();
    }
    
    [HttpGet]
    [Route("c")]
    public async Task<IActionResult> c()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName));


        return Ok();
    }
}