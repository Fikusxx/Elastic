using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("search")]
public class Search : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();

    /// <summary>
    /// full text queries are analyzed, ie run against tokenizer
    /// </summary>
    [HttpGet]
    [Route("match-all")]
    public async Task<IActionResult> MatchAll()
    {
        // "query": {
        //     "match_all": { }
        // }

        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants
                .IndexName) // doesnt work w/o explicit index, returns all document from ALL indexes xd
            .Query(q => q
                .MatchAll(m => { })
            ));

        return Ok(result.Documents);
    }

    /// <summary>
    /// Term() best works with Keyword type properties, ie EXACT MATCH values in an index, like Id, enum, etc
    /// Such queries are not analyzed, hence exact values search
    /// </summary>
    [HttpGet]
    [Route("term")]
    public async Task<IActionResult> Term()
    {
        // "query": {
        //     "term": {
        //         "title": {
        //             "value": "ori"
        //         }
        //     }
        // }

        // title is Text() field, thus "standard" tokenizer (by default) lowercased all values of that field
        // and cuz Term() is run against EXACT MATCH in an index - only lower case "ori" is present there
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Term(m => m
                    .Field(f => f.Title).Value("ori"))
            ));

        return Ok(result.Documents);
    }

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
    [Route("terms")]
    public async Task<IActionResult> Terms()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Terms(t => t
                    .Field(f => f.Title)
                    .Term(new TermsQueryField([FieldValue.String("hollow"), FieldValue.String("ori")])))
            ));

        return Ok(result.Documents);
    }

    [HttpGet]
    [Route("ids")]
    public async Task<IActionResult> Ids()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Ids(new IdsQuery { Values = new Ids([1, 3, 5]) })
            ));

        return Ok(result.Documents);
    }
}