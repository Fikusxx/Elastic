using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

/// <summary>
/// there's also parent/child syntax which requires joins, complex and overall bs
/// no schema, complex routing, both parent and children are stored within one index, just use nested really
/// https://stackoverflow.com/questions/30997929/how-to-search-in-elasticsearch-nested-objects-using-nest
/// </summary>
[ApiController]
[Route("relations")]
public class Relations : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();
    private const string RelationsIndex = "relations";

    /// <summary>
    /// use Nested() for querying inside nested objects
    /// reason why it doesnt work without Nested() is to how nested objects are stored within Lucene index
    /// </summary>
    [HttpGet]
    [Route("get-all")]
    public async Task<IActionResult> GetAll()
    {
        // works just fine
        var resultWithNested = await client.SearchAsync<Outer>(indices: RelationsIndex, cfg => cfg
            .Query(q => q
                .Nested(n => n
                    .Path(f => f.Inners)
                    .Query(qu => qu
                        .Match(m => m
                            .Field(f => f.Inners.First().Gender).Query("m"))
                    ))));

        // returns empty []
        var resultWithoutNested = await client.SearchAsync<Outer>(indices: RelationsIndex, cfg => cfg
            .Query(q => q
                .Bool(b => b
                    .Must(mu => mu
                        .Match(m => m
                            .Field(f => f.Inners.First().Gender).Query("m"))
                    ))));

        return Ok(new { Nested = resultWithNested.Documents, WithoutNested = resultWithoutNested.Documents });
    }

    [HttpPost]
    [Route("index")]
    public async Task<IActionResult> CreateIndex()
    {
        var result = await client.Indices.CreateAsync<Outer>(index: RelationsIndex, cfg => cfg
            .Mappings(m => m
                .Properties(p => p
                    .Keyword(f => f.Id)
                    .Text(f => f.Name)
                    .Nested(f => f.Inners, descriptor => { descriptor.Dynamic(DynamicMapping.True); })
                )));

        return Ok(result);
    }

    [HttpPost]
    [Route("populate")]
    public IActionResult Populate()
    {
        Outer[] dummy =
        [
            new Outer { Id = 1, Name = "Outer #1", Inners = [new Inner { Gender = "M" }] },
            new Outer { Id = 2, Name = "Outer #2", Inners = [new Inner { Gender = "F" }] },
            new Outer { Id = 3, Name = "Outer #3", Inners = [new Inner { Gender = "M" }] },
            new Outer { Id = 4, Name = "Outer #4", Inners = [new Inner { Gender = "F" }] }
        ];

        var bulkAll = client.BulkAll(dummy,
            config => config
                .Index(RelationsIndex)
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
        var result = await client.Indices.DeleteAsync<Outer>(RelationsIndex);

        return Ok(result);
    }
}

public class Outer
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public List<Inner> Inners { get; init; } = [];
}

public class Inner
{
    public required string Gender { get; init; }
}