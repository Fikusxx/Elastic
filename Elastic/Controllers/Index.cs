using Elastic.Clients.Elasticsearch;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

[ApiController]
[Route("index")]
public sealed class Index : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();

    /// <summary>
    /// Each shard is an index. Shard may contain up to 2B records
    /// </summary>
    [HttpPost]
    [Route("create-index")]
    public async Task<IActionResult> CreateIndex()
    {
        var response = await client.Indices.CreateAsync<Game>(ElasticConstants.IndexName, x =>
        {
            x.Mappings(m => m
                    .Properties(p => p // also .Boolean(), .Date(), .Ip(), GeoPoint(), etc
                        .Keyword(n => n.Id) // used for exact matching, i.e id, enum, etc
                        .Text(n => n.Title) // used for full text searches within its content
                        .IntegerNumber(n => n.Price))) // also short, long, float, double

                // default true, coercion disabled, thus supplying values 7.5 || "7.5"
                // wont be automatically converted to original data type and will result in an error
                // .IntegerNumber(n => n.Price, y => y.Coerce(false)))) 

                // inner objects are flattened, i.e inner.value cuz Apache Lucene doesnt support nested objects
                // in case of an array - values are grouped inner.value1 : [ 1, 2 ] && inner.value2 : [ "text1", "text2" ] 
                // thus queries like value1 == 1 AND value2 == "text" converted into OR, there's no correlation between values in such arrays
                // .Object(n => n.InnerObject, descriptor => 
                // {
                //     descriptor.Properties(innerP =>
                //     {
                //         innerP.Text(...)
                //     });
                // }))) 

                // for cases with arrays like above - there's Nested mapping
                // such nested objects will be stored as separate (hidden) documents
                // .Nested(n => ...
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0) // requires >1 nodes, default 1

                    // default 60s. Old optimistic concurrency control mechanism.
                    // Defines for how long _version value is kept when document is deleted.
                    // For scenarios when a new document with the same id is created right away
                    .GcDeletes(TimeSpan.FromSeconds(60)));
        });

        return Ok(response);
    }

    [HttpDelete]
    [Route("delete-indexes")]
    public async Task<IActionResult> DeleteIndex()
    {
        var response1 = await client.Indices.DeleteAsync(ElasticConstants.IndexName);
        var response2 = await client.Indices.DeleteAsync(ElasticConstants.ComplexIndexName);

        return Ok(new { response1, response2 });
    }
    
    [HttpPost]
    [Route("create-complex-index")]
    public async Task<IActionResult> CreateComplexIndex()
    {
        var response = await client.Indices.CreateAsync<ComplexType>(ElasticConstants.ComplexIndexName, x =>
        {
            // adding a mapping for a field DOES NOT make it required
            // all fields are options and such validations should be at the app layer
            x.Mappings(m => m
                    .Properties(p => p

                        // used for exact matching, i.e id, enum, etc   
                        .Keyword(n => n.Id)
                        .Keyword(n => n.Enum)

                        // used for full text searches within its content
                        // also used for collection of strings as they're flattened
                        .Text(n => n.Description)
                        .Text(n => n.Tags)
                        .ShortNumber(n => n.Short)
                        .IntegerNumber(n => n.Int)

                        // default true, coercion disabled, thus supplying values 7.5 || "7.5"
                        // wont be automatically converted to original data type and will result in an error
                        .FloatNumber(n => n.Float, config => config.Coerce(false))
                        .DoubleNumber(n => n.Double, config => config.Coerce(false))
                        .Boolean(n => n.Bool)
                        .Ip(n => n.IpAddress)
                        .GeoPoint(n => n.GeoPoint)
                        
                        // dates are converted to utc and stored as epoch internally (1273172390123712903)
                        .Date(n => n.DateOnly)
                        .Date(n => n.DateTime)

                        // inner objects are flattened, i.e inner.value cuz Apache Lucene doesnt support nested objects
                        // in case of an array - values are grouped inner.value1 : [ 1, 2 ] && inner.value2 : [ "text1", "text2" ] 
                        // thus queries like value1 == 1 AND value2 == "text" converted into OR, there's no correlation between values in such arrays
                        .Object(n => n.InnerOne, descriptor =>
                        {
                            descriptor.Properties(innerProps =>
                            {
                                innerProps.Text(n => n.InnerOne.Value);
                                innerProps.IntegerNumber(n => n.InnerOne.Values);
                                // other props
                            });
                        })

                        // for cases with arrays like above - there's Nested mapping
                        // such nested objects will be stored as separate (hidden) documents
                        .Nested(n => n.InnerTwos, config =>
                        {
                            config.Properties(innerProps =>
                            {
                                // innerProps.Text(n => n.InnerTwos)
                            });
                        })
                    ))
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0) // requires >1 nodes, default 1

                    // default 60s. Old optimistic concurrency control mechanism.
                    // Defines for how long _version value is kept when document is deleted.
                    // For scenarios when a new document with the same id is created right away
                    .GcDeletes(TimeSpan.FromSeconds(60)));
        });

        return Ok(response);
    }

    /// <summary>
    /// see ComplexType json files
    /// </summary>
    [HttpPost]
    [Route("create-complex-document")]
    public async Task<IActionResult> CreateComplex()
    {
        var complexType = new ComplexType
        {
            Id = 1,
            Short = 1,
            Int = 1,
            Float = 1.0f,
            Double = 1.0,
            Description = "my description",
            Tags = ["tag1", "tag2"],
            Enum = ComplexTypeNum.One,
            DateTime = DateTime.Now,
            DateOnly = DateOnly.FromDateTime(DateTime.Now),
            Bool = true,
            GeoPoint = [55.55, 66.66],
            IpAddress = "127.0.0.1",
            InnerOne = new InnerOne { Value = "inner one value", Values = [1, 2, 3] },
            InnerTwos = [new InnerTwo { Value = "inner two value #1" }, new InnerTwo { Value = "inner two value #2" }]
        };

        var response = await client.IndexAsync(document: complexType, index: ElasticConstants.ComplexIndexName);

        return Ok(response);
    }
}