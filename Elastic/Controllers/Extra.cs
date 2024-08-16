using Elastic.Clients.Elasticsearch;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("extra")]
public class Extra : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();
    private const string ExtraIndex = "extra";

    /// <summary>
    /// Requires all words (tokens) in a query to be present.
    /// MatchPhrase() takes into account words offset (ie order) when text is analyzed when indexing.
    /// Slop defines allowed offset (how far apart) between words (tokens),
    /// ie slop = 0 means words in a query should next to each other in the original index
    /// ALSO: offset could be in any order, for example: hello world == world hello
    /// ALSO: better proximity affects relevance score
    /// </summary>
    [HttpGet]
    [Route("proximity")]
    public async Task<IActionResult> Proximity([FromQuery] int? slop)
    {
        var result = await client.SearchAsync<Simple>(x => x
            .Index(ExtraIndex)
            .Query(q => q
                .MatchPhrase(mp => mp
                    .Field(f => f.Title)
                    .Query("hello world")
                    .Slop(slop ?? 0)
                )));

        // find documents that contain "hello world" tokens and 
        // also boost their score based on how close these tokens are located within Lucene index
        var resultWithBetterScore = await client.SearchAsync<Simple>(x => x
            .Index(ExtraIndex)
            .Query(q => q
                .Bool(b => b
                    .Must(mu => mu
                        .Match(m => m
                            .Field(f => f.Title)
                            .Query("hello world")))
                    .Should(s => s
                        .MatchPhrase(mp => mp
                            .Field(f => f.Title)
                            .Query("hello world")
                            .Slop(2)))))

            // just for clarity
            .Sort(s => s.Score(c => c.Order(SortOrder.Asc)))
        );

        return Ok(new { Normal = result.Documents, Better = resultWithBetterScore.Documents });
    }

    /// <summary>
    /// https://www.elastic.co/guide/en/elasticsearch/reference/8.14/common-options.html#fuzziness
    /// Length:Edits => 1-2:0 || 3-5:1 || >5:2
    /// maximum allowed edit number is 2, due to some studies idk
    /// </summary>
    [HttpGet]
    [Route("fuzzy")]
    public async Task<IActionResult> Fuzzy([FromQuery] string? value)
    {
        var match = await client.SearchAsync<Simple>(x => x
            .Index(ExtraIndex)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Title)
                    .Query(value ?? "hello")
                    .Fuzziness(new Fuzziness("auto"))

                    // default - true
                    // example words: live | lvie , because default fuzziness for this word is 1,
                    // thus if we'd use default Levenshtein algo then result - live != lvie
                    // however, Elastic uses transposition which states that if 2 subsequent letters are swapped, that'd count as 1 fuzziness
                    // therefore with it enabled result - live == lvie
                    .FuzzyTranspositions(false)
                )));

        // Fuzzy() query IS NOT analyzed (exact match) and has fuzziness set to "auto" by default
        var fuzzy = await client.SearchAsync<Simple>(x => x
            .Index(ExtraIndex)
            .Query(q => q
                .Fuzzy(fu => fu
                    .Field(f => f.Title)
                    .Value(value ?? "hello")
                )));

        return Ok(new { Match = match.Documents, Fuzzy = fuzzy.Documents });
    }

    [HttpGet]
    [Route("b")]
    public async Task<IActionResult> b()
    {
        var result = await client.SearchAsync<Simple>(x => x
            .Index(ExtraIndex));


        return Ok();
    }

    [HttpGet]
    [Route("c")]
    public async Task<IActionResult> c()
    {
        var result = await client.SearchAsync<Simple>(x => x
            .Index(ExtraIndex));


        return Ok();
    }

    [HttpPost]
    [Route("populate")]
    public IActionResult Populate()
    {
        Simple[] dummy =
        [
            new Simple { Title = "Hello World" },
            new Simple { Title = "Hello Wonderful World" },
            new Simple { Title = "Hello Wonderful Amazing World" },
            new Simple { Title = "Hello Wonderful Amazing Precious World" },
        ];

        var bulkAll = client.BulkAll(dummy,
            config => config
                .Index(ExtraIndex)
                .BackOffRetries(backOffRetries: 5)
                .BackOffTime(TimeSpan.FromSeconds(10))
                .ContinueAfterDroppedDocuments(proceed: true)
                .DroppedDocumentCallback((item, _) => { Console.WriteLine(item.Error?.Reason); })
                .MaxDegreeOfParallelism(Environment.ProcessorCount)
                .Size(size: 100));

        var response = bulkAll.Wait(TimeSpan.FromMinutes(1), _ => { Console.WriteLine("Done"); });

        return Ok(response);
    }

    [HttpDelete]
    [Route("purge")]
    public async Task<IActionResult> Purge()
    {
        var result = await client.Indices.DeleteAsync<Simple>(ExtraIndex);

        return Ok(result);
    }
}

public class Simple
{
    public required string Title { get; set; }
}