using Elastic.Clients.Elasticsearch;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("compound-search")]
public class CompoundSearch : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();

    /// <summary>
    /// Must - Match() with explicit AND
    /// </summary>
    [HttpGet]
    [Route("multi-term")]
    public async Task<IActionResult> MultiTerm()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Bool(b => b
                    .Must(mu =>
                            mu.Term(t => t.Field(f => f.Title).Value("ori")),
                        mu =>
                            mu.Term(t => t.Field(f => f.Price).Value(777)))
                )));

        return Ok(result.Documents);
    }

    [HttpGet]
    [Route("compound-query")]
    public async Task<IActionResult> CompoundQuery()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Bool(b => b

                    // must() defines HOW WELL document is matched, ie "relevance score"
                    // also makes it easier for caching as only _score matters, other queries are just somewhat "filters"
                    .Must(mu => mu
                        .Match(m => m
                            .Field(f => f.Title).Query("ori")))

                    // has no impact on score, placed outside of Must()
                    .MustNot(mn => mn
                        .Match(m => m
                            .Field(f => f.Title).Query("hollow")))

                    // optional, no impact on # of returned documents.
                    // can think of it as "i prefer this value to be there, but dont require it"
                    // boosts relevance score of found documents if such value(s) found
                    .Should(s => s
                        .Match(m => m
                            .Field(f => f.Title).Query("whatever")))

                    // there's no reason to place YES/NO queries inside a Must() as they have no impact on score
                    .Filter(filter => filter
                        .Range(r => r
                            .NumberRange(nr => nr
                                .Field(f => f.Price).Gte(100)))
                    ))));

        return Ok(result.Documents);
    }
}