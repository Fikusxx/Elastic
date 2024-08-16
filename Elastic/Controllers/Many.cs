using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("many")]
public sealed class Many : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAll()
    {
        // implicit MatchAll()
        var simple = await client.SearchAsync<Game>(indices: ElasticConstants.IndexName);

        var matchAll = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q.MatchAll(_ => { })));

        var request = new SearchRequest(ElasticConstants.IndexName)
        {
            Query = new MatchAllQuery()
        };

        var searchRequest = await client.SearchAsync<Game>(request);

        return Ok(new { Simple = simple.Documents, MatchAll = matchAll.Documents, SearchRequest = searchRequest.Documents });
    }

    [HttpPost]
    [Route("bulk")]
    public IActionResult Bulk()
    {
        Game[] dummyGames =
        [
            new Game { Id = 2, Title = "Ori", Price = 10 },
            new Game { Id = 3, Title = "Ori", Price = 20 },
            new Game { Id = 4, Title = "Ori", Price = 30 },
            new Game { Id = 5, Title = "Ori", Price = 40 },
        ];

        // run async in the background right away
        var bulkAll = client.BulkAll(dummyGames,
            config => config
                .Index(ElasticConstants.IndexName)
                .BackOffRetries(backOffRetries: 5)
                .BackOffTime(TimeSpan.FromSeconds(10))
                .ContinueAfterDroppedDocuments(proceed: true)
                .DroppedDocumentCallback((item, _) => { Console.WriteLine(item.Error?.Reason); })
                .MaxDegreeOfParallelism(Environment.ProcessorCount)
                .Size(size: 100));

        // thus needs to be "awaited"
        var response = bulkAll.Wait(TimeSpan.FromMinutes(1), _ => { Console.WriteLine("Done"); });

        return Ok(response);
    }

    [HttpPut]
    [Route("query")]
    public async Task<IActionResult> UpdateByQuery()
    {
        var response = await client.UpdateByQueryAsync<Game>(indices: ElasticConstants.IndexName, x =>
        {
            x.Query(q => q
                    .Match(m => m
                        .Field(f => f.Title).Query("Ori")))
                .Script(s => s
                    .Source("ctx._source.price = params.price;")
                    .Lang(ScriptLanguage.Painless)
                    .Params(p => p.Add("price", 777)))
                .Conflicts(Conflicts.Proceed); // is NOT transactional, ie partial failures are NOT rolled back
        });

        return Ok(response);
    }

    [HttpDelete]
    [Route("query")]
    public async Task<IActionResult> DeleteByQuery()
    {
        var response = await client.DeleteByQueryAsync<Game>(indices: ElasticConstants.IndexName, x =>
        {
            x.Query(q => q
                    .Match(m => m
                        .Field(f => f.Title).Query("Ori")))
                .Conflicts(Conflicts.Proceed); // is NOT transactional, ie partial failures are NOT rolled back
        });

        return Ok(response);
    }
}