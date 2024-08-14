using Elastic.Clients.Elasticsearch;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("single")]
public sealed class Single : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();

    [HttpGet]
    public async Task<IActionResult> GetSingle()
    {
        var response = await client.GetAsync<Game>(id: 1);

        if (response.IsValidResponse)
        {
            var doc = response.Source;
        }

        return Ok(response.Source);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecord()
    {
        var game = new Game
        {
            Id = 1,
            Title = "Ori",
            Price = 69
        };

        var response = await client.IndexAsync<Game>(document: game);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update()
    {
        var resource = await client.GetAsync<Game>(id: 1);
        var game = resource.Source;
        game!.Title = "Hollow Knight";
        
        var response = await client.UpdateAsync<Game, Game>(id: 1, x =>
        {
            x.Doc(game);
            x.IfPrimaryTerm(resource.PrimaryTerm); // optimistic concurrency
            x.IfSeqNo(resource.SeqNo); // optimistic concurrency
        });

        return Ok(response);
    }


    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var response = await client.DeleteAsync<Game>(id: 1);

        return Ok(response);
    }
}