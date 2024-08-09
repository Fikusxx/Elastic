using Elastic.Clients.Elasticsearch;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("base")]
public sealed class Base : ControllerBase
{
    private const string indexName = "my_index";
    private static readonly Guid Id = Guid.Parse("82157646-b752-4899-904d-562dfd02f20c");
    private readonly ElasticsearchClient client;

    public Base()
    {
        var settings = new ElasticsearchClientSettings(); // defaults to http://localhost:9200
        this.client = new ElasticsearchClient(settings);
    }

    [HttpGet]
    [Route("ping")]
    public async Task<IActionResult> Ping()
    {
        var response = await client.PingAsync();

        return Ok(response);
    }

    [HttpGet]
    [Route("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var response = await client.SearchAsync<Game>(x => x
            .Index(indexName)
            .From(0)
            .Size(10)
            .Query(query => query.Match(t => t.Query("123"))));

        if (response.IsValidResponse)
        {
            var doc = response.Documents.FirstOrDefault();
        }

        return Ok(response);
    }

    [HttpGet]
    [Route("get-single")]
    public async Task<IActionResult> GetSingle()
    {
        var response = await client.GetAsync<Game>(1, index => index.Index(indexName));

        if (response.IsValidResponse)
        {
            var doc = response.Source;
        }

        return Ok(response.Source);
    }

    [HttpPost]
    [Route("create-index")]
    public async Task<IActionResult> CreateIndex()
    {
        var response = await client.Indices.CreateAsync(indexName);

        return Ok(response);
    }

    [HttpPost]
    [Route("create-single")]
    public async Task<IActionResult> CreateRecord()
    {
        var game = new Game
        {
            Id = 1,
            Title = "Ori",
            Price = 69
        };

        var response = await client.IndexAsync(game, index: indexName);

        return Ok(response);
    }

    [HttpDelete]
    [Route("delete-single")]
    public async Task<IActionResult> Delete()
    {
        var response = await client.DeleteAsync(index: indexName ,id: 1);

        return Ok(response);
    }
}