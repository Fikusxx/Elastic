using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("match-internals")]
public class MatchInternals : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();

    /// <summary>
    /// VERY IMPORTANT
    /// Match() with Implicit OR - internally creates Bool() with Should() with n Term() queries,
    /// where query is analyzed and n depends on how many words in a query
    /// Match() with Explicit AND - internally creates Bool() with Must() with n Term() queries,
    /// where query is analyzed and n depends on how many words in a query
    /// </summary>
    [HttpGet]
    [Route("match-operators")]
    public async Task<IActionResult> MatchOperators()
    {
        // implicit OR, title should contain either "hollow" OR "ori"
        var resultWithImplicitOr = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Title).Query("hollow ori")
                )));

        // explicit AND, title should contain both "hollow" and "ori"
        var resultWithExplicitAnd = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Title).Query("hollow ori").Operator(Operator.And)
                )));

        return Ok(new
        {
            Or = resultWithImplicitOr.Documents,
            And = resultWithExplicitAnd.Documents
        });
    }

    /// <summary>
    /// By default Should() is optional and only boosts _score of a document if Must() or Filter() is specified in a Bool() query
    /// It becomes mandatory, ie return only documents with a value specified there, if above mentioned queries are not present
    /// </summary>
    [HttpGet]
    [Route("should")]
    public async Task<IActionResult> Should()
    {
        // internally this query is absolutely the same as below
        // such query internally is Analyzed => Bool() => Should() => Term()
        // var result = await client.SearchAsync<Game>(x => x
        //     .Index(ElasticConstants.IndexName)
        //     .Query(q => q
        //         .Match(m => m
        //             .Field(f => f.Title).Query("HOLLOW"))
        //     ));

        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Bool(b => b
                    .Should(s => s
                        .Match(m => m
                            .Field(f => f.Title).Query("HOLLOW"))
                    ))));

        return Ok(result.Documents);
    }

    /// <summary>
    /// Must - Match() with explicit AND
    /// </summary>
    [HttpGet]
    [Route("match")]
    public async Task<IActionResult> Match()
    {
        // this query is internally the same as below
        // such query internally is Analyzed => Bool() => Must() => Term()
        // var result = await client.SearchAsync<Game>(x => x
        //     .Index(ElasticConstants.IndexName)
        //     .Query(q => q
        //         .Match(m => m
        //             .Field(f => f.Title).Query("hollow knight").Operator(Operator.And))
        //     ));

        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Bool(b => b
                    .Must(mu =>
                            mu.Term(t => t.Field(f => f.Title).Value("hollow")),
                        mu =>
                            mu.Term(t => t.Field(f => f.Title).Value("knight")))
                )));

        return Ok(result.Documents);
    }
}