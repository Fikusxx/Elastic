using Elastic.Clients.Elasticsearch;
using Elastic.Common;
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
}