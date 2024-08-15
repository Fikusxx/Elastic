using Elastic.Clients.Elasticsearch;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("filter")]
public class Filter : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();

    [HttpGet]
    [Route("select")]
    public async Task<IActionResult> Select()
    {
        var result = await client.SearchAsync<dynamic>(cfg => cfg
            .Index(ElasticConstants.IndexName)
            .SourceIncludes(Fields.FromStrings(["id", "title"]))
            // .Source(new SourceConfig(Fetch: false)) // doesnt fetch any data from _source
            .Query(q => q
                .MatchAll(_ => { })
            ));

        return Ok(result.Documents);
    }

    /// <summary>
    /// 10k documents is the limit of Size()
    /// </summary>
    [HttpGet]
    [Route("pagination")]
    public async Task<IActionResult> Pagination()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .From(0) // offset (skip)
            .Size(2) // limit (take)
            .Query(q => q
                .MatchAll(_ => { }))
            .Sort(sort => sort.Field(f => f.Price, c => c
                    .Order(SortOrder.Asc))
                // .Mode(SortMode.Avg)) // can only be applied for array of numeric values
            ));

        return Ok(result.Documents);
    }

    /// <summary>
    /// Ran against numeric, date, exact values
    /// Doesnt impact score relevance
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("filter")]
    public async Task<IActionResult> Filtering()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Bool(b => b
                    .Filter(filter => filter
                        .Range(r => r
                            .NumberRange(nr => nr
                                .Field(f => f.Price).Gte(100).Lte(1000)))
                    ))));

        return Ok(result.Documents);
    }
}