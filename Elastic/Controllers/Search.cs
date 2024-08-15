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
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName) // doesnt work w/o explicit index, returns documents from ALL indexes
            .Query(q => q
                .MatchAll(_ => { })
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
    
    /// <summary>
    /// SQL analogy is as for IN (value1, value2, ...) query
    /// </summary>
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

    /// <summary>
    /// SQL analogy is as for IN (value1, value2, ...) query
    /// </summary>
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

    /// <summary>
    /// Also supports DateRange, default format yyyy-MM-dd, which can be changed
    /// </summary>
    [HttpGet]
    [Route("range")]
    public async Task<IActionResult> Range()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Range(r => r
                    .NumberRange(nr => nr
                        .Field(f => f.Price).Gte(100).Lte(1000))
                )));

        return Ok(result.Documents);
    }

    /// <summary>
    /// Can be run against collections to see if there are any elements
    /// or against values that might not be present due to dynamic mapping
    /// </summary>
    [HttpGet]
    [Route("exists")]
    public async Task<IActionResult> Exists()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Exists(e => e.Field(f => f.Title))
            ));

        return Ok(result.Documents);
    }

    [HttpGet]
    [Route("prefix")]
    public async Task<IActionResult> Prefix()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .Prefix(p => p.Field(f => f.Title).Value("h"))
            ));

        return Ok(result.Documents);
    }

    /// <summary>
    /// Slow queries, as they could be ran against a large volumes of data, especially if pattern matching is at the begging of query, like "*ing"
    /// * - for any number of characters
    /// ? - for a single character
    /// </summary>
    [HttpGet]
    [Route("wildcard")]
    public async Task<IActionResult> Wildcard()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                    .Wildcard(w => w
                        .Field(f => f.Title).Value("h*w")) // as for hollow
            ));

        return Ok(result.Documents);
    }

    /// <summary>
    /// Might be useful for when a customer search for documents with a value somewhere within
    /// Internally, Elastic executes match query for each specified field
    /// </summary>
    [HttpGet]
    [Route("match-multi")]
    public async Task<IActionResult> MatchMulti()
    {
        var result = await client.SearchAsync<Game>(x => x
            .Index(ElasticConstants.IndexName)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(Fields.FromFields([Field.FromString("title"), Field.FromString("description")]))
                    .Query("ori")
                )));

        return Ok(result.Documents);
    }
}