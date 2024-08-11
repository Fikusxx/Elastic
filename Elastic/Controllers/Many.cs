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
    [Route("lambda")]
    public async Task<IActionResult> Lambda()
    {
        // Term() ONLY works with Keyword type properties, ie fixed values, like Id, enum, etc
        // Match() works with other types, ie like Where() linq
        
        var response = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .From(0)
            .Size(10)
            // .Query(query => query.Term(t =>
            //     t.Field(f => f.Id)
            //         .Value(1))));
            .Query(query => query.Match(t =>
                t.Field(f => f.Title).Query("Ori"))));

        return Ok(response.Documents);
    }

    [HttpGet]
    [Route("search-request")]
    public async Task<IActionResult> SearchRequest()
    {
        var request = new SearchRequest(ElasticConstants.IndexName)
        {
            From = 0,
            Size = 10,
            Query = new MatchQuery("title"!) { Query = "Ori" },
            // Query = new TermQuery("title"!) { Value = "Ori" }
        };

        var response = await client.SearchAsync<Game>(request);

        return Ok(response.Documents);
    }

    [HttpGet]
    [Route("bool")]
    public async Task<IActionResult> Bool()
    {
        var response = await client.SearchAsync<Game>(x =>
        {
            x.Index(ElasticConstants.IndexName);
            x.Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        // .Term(t => t.Field(field => field.Title).Value("Ori")))));
                        .Match(t => t.Field(field => field.Title).Query("Ori")))));
            x.Size(20);
            x.Sort(sort => sort.Field(f => f.Price, c => c.Order(SortOrder.Asc)));
        });

        return Ok(response.Documents);
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
                .DroppedDocumentCallback((item, game) => { Console.WriteLine(item.Error?.Reason); })
                .MaxDegreeOfParallelism(Environment.ProcessorCount)
                .Size(size: 100));

        // thus needs to be "awaited"
        var response = bulkAll.Wait(TimeSpan.FromMinutes(1), x => { Console.WriteLine("Done"); });

        return Ok(response);
    }
}