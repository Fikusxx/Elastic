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
        var response = await client.GetAsync<Game>(id: 1, index => index.Index(ElasticConstants.IndexName));
        // var response = await client.GetAsync<Game>(id: ElasticConstants.Id.ToString(), index => index.Index(ElasticConstants.IndexName));

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
            // Id = ElasticConstants.Id.ToString(),
            Id = 1,
            Title = "Ori",
            Price = 69
        };

        var response = await client.IndexAsync(document: game, index: ElasticConstants.IndexName);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update()
    {
        var game = (await client.GetAsync<Game>(id: 1, index => index.Index(ElasticConstants.IndexName))).Source;
        // var game = (await client.GetAsync<Game>(id: ElasticConstants.Id.ToString(), index => index.Index(ElasticConstants.IndexName))).Source;
        game!.Title = "Hollow Knight";

        var response = await client.UpdateAsync<Game, Game>(index: ElasticConstants.IndexName, id: 1, x => x.Doc(game));
        // var response = await client.UpdateAsync<Game, Game>(index: ElasticConstants.IndexName, id: ElasticConstants.Id.ToString(), x => x.Doc(game));

        return Ok(response);
    }


    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var response = await client.DeleteAsync(index: ElasticConstants.IndexName, id: 1);
        // var response = await client.DeleteAsync(index: ElasticConstants.IndexName, id: ElasticConstants.Id.ToString());

        return Ok(response);
    }
}