using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Common;
using Microsoft.AspNetCore.Mvc;

namespace Elastic.Controllers;

/// <summary>
/// https://www.elastic.co/guide/en/elasticsearch/client/net-api/1.x/aggregations.html
/// </summary>
[ApiController]
[Route("aggregations")]
public class Aggregations : ControllerBase
{
    private readonly ElasticsearchClient client = ElasticConstants.GetClient();
    private const string OrdersIndex = "orders";

    [HttpPost]
    [Route("create-index")]
    public async Task<IActionResult> CreateIndex()
    {
        var result = await client.Indices.CreateAsync<Order>(index: OrdersIndex, cfg => cfg
            .Mappings(m => m
                .Properties(p => p
                    .Keyword(f => f.Id)
                    .Date(f => f.PurchasedAt)
                    .Keyword(f => f.Status)
                    .Keyword(f => f.Channel)
                    .DoubleNumber(f => f.Total)
                    .Object(o => o.Manager, d => d.Dynamic(DynamicMapping.True))
                    .Nested(f => f.Items, d => d.Dynamic(DynamicMapping.True))
                )));

        return Ok(result);
    }

    [HttpPost]
    [Route("populate")]
    public IActionResult Populate()
    {
        Order[] dummy =
        [
            new Order
            {
                Id = 1, PurchasedAt = DateTime.Now, Status = "ON", Channel = "1", Total = 100,
                Manager = new Manager { Id = 1, Name = "Tom" },
                Items = [new LineItem { ProductId = 1, Quantity = 10, Price = 50 }]
            },
            new Order
            {
                Id = 2, PurchasedAt = DateTime.Now, Status = "OFF", Channel = "2", Total = 1000,
                Manager = new Manager { Id = 2, Name = "Alice" },
                Items = [new LineItem { ProductId = 2, Quantity = 10, Price = 500 }]
            },
            new Order
            {
                Id = 3, PurchasedAt = DateTime.Now, Status = "ON", Channel = "1", Total = 100,
                Manager = new Manager { Id = 1, Name = "Tom" },
                Items = [new LineItem { ProductId = 3, Quantity = 10, Price = 50 }]
            },
        ];

        var bulkAll = client.BulkAll(dummy,
            config => config
                .Index(OrdersIndex)
                .BackOffRetries(backOffRetries: 5)
                .BackOffTime(TimeSpan.FromSeconds(10))
                .ContinueAfterDroppedDocuments(proceed: true)
                .DroppedDocumentCallback((item, _) => { Console.WriteLine(item.Error?.Reason); })
                .MaxDegreeOfParallelism(Environment.ProcessorCount)
                .Size(size: 100));

        var response = bulkAll.Wait(TimeSpan.FromMinutes(1), _ => { Console.WriteLine("Done"); });

        return Ok(response);
    }

    [HttpGet]
    [Route("numeric")]
    public async Task<IActionResult> Numeric()
    {
        var result = await client.SearchAsync<Order>(x => x
            .Index(OrdersIndex)

            // no need for returning _source documents
            .Size(0)

            // top level filter, aggregations will run against selected documents
            .Query(q => q.MatchAll(_ => { }))

            // Aggregations
            .Aggregations(a => a

                    // total sum, enough said
                    .Add("total_amount", agg => agg
                        // aggregation level filter, this aggregation will be run against filtered result
                        // .Filter()
                        .Sum(t => t.Field(f => f.Total)))

                    // approximate count of distinct values
                    .Add("total_managers", agg => agg
                        .Cardinality(t => t.Field(f => f.Manager.Id)))

                    // count() basically
                    .Add("total_orders", agg => agg
                        .ValueCount(t => t.Field(f => f.Id)))

                    // combined result with: count | min | max | avg | sum
                    .Add("stats", agg => agg
                        .Stats(t => t.Field(f => f.Total)))

                // alternate syntax
                // .Aggregations(a =>
                // {
                //     a["total_amount"] = new AggregationDescriptor<Order>().Sum(y => y.Field(f => f.Total));
                //     return a;
                // })
            ));

        // other aggregations...
        var totalAmount = result.Aggregations?.GetSum("total_amount")?.Value;
        var totalManagers = result.Aggregations?.GetCardinality("total_managers")?.Value;
        var totalOrders = result.Aggregations?.GetValueCount("total_orders")?.Value;
        var stats = result.Aggregations?.GetStats("stats");

        return Ok(new { totalAmount, totalManagers, totalOrders, stats });
    }

    [HttpGet]
    [Route("bucket")]
    public async Task<IActionResult> Bucket()
    {
        var result = await client.SearchAsync<Order>(x => x
            .Index(OrdersIndex)
            .Size(0)
            .Aggregations(a => a

                // groupBy(x => x.Status).Where(x => x.Count() >= 0)
                // DocCount might be inaccurate if documents are distributes across many shards
                // accuracy increases the higher Size() is
                .Add("status", ag => ag
                    .Terms(t => t
                        .Field(f => f.Status)
                        .MinDocCount(0)))
            ));

        var statusTerms = result.Aggregations?
            .GetStringTerms("status")?.Buckets
            .Select(x => new { x.DocCount, x.Key });

        return Ok(new { statusTerms });
    }

    [HttpGet]
    [Route("nested")]
    public async Task<IActionResult> Nested()
    {
        var result = await client.SearchAsync<Order>(x => x
            .Index(OrdersIndex)
            .Size(0)
            .Aggregations(a => a

                // outer aggregation
                .Add("status", ag => ag
                    .Terms(t => t.Field(f => f.Status))

                    // inner aggregation within a bucket
                    .Aggregations(innerA => innerA
                        .Add("stats", innerAg => innerAg
                            .Stats(s => s.Field(f => f.Total)))))
            ));

        var statusTerms = result.Aggregations?
            .GetStringTerms("status")?.Buckets
            .Select(x => new { x.DocCount, x.Key, Stats = x.Aggregations.GetStats("stats") });

        return Ok(new { statusTerms });
    }

    /// <summary>
    /// Dates are stored as unix MILLISECONDS not SECONDS
    /// </summary>
    [HttpGet]
    [Route("ranges")]
    public async Task<IActionResult> Ranges()
    {
        var result = await client.SearchAsync<Order>(x => x
            .Index(OrdersIndex)
            .Size(0)

            // from included, to excluded
            .Aggregations(a => a
                .Add("numeric_range", ag => ag
                    .Range(r => r
                        .Field(f => f.Total)
                        .Ranges(ra => ra
                            .From(100).To(1000))))

                // Dates are stored as unix MILLISECONDS not SECONDS
                .Add("date_range", ag => ag
                    .DateRange(r => r
                        .Field(f => f.PurchasedAt)
                        .Ranges(ra => ra
                            .From(new FieldDateMath(DateTimeOffset.Now.AddMonths(-1).ToUnixTimeMilliseconds()))
                            .To(new FieldDateMath(DateTimeOffset.Now.AddMonths(1).ToUnixTimeMilliseconds()))))
                )));

        var numeric = result.Aggregations?.GetRange("numeric_range")?.Buckets
            .Select(x => new { x.Key, x.DocCount, x.From, x.To });
        var date = result.Aggregations?.GetDateRange("date_range")?.Buckets
            .Select(x => new { x.Key, x.DocCount, x.From, x.To });

        return Ok(new { numeric, date });
    }

    [HttpGet]
    [Route("histogram")]
    public async Task<IActionResult> Histogram()
    {
        var result = await client.SearchAsync<Order>(x => x
            .Index(OrdersIndex)
            .Size(0)
            .Aggregations(a => a
                .Add("histogram", ag => ag
                    .Histogram(h => h
                        .Field(f => f.Total)
                        .Interval(100)
                        .MinDocCount(0)
                        // buckets are created within these bounds, regardless whether or not any documents fall into them
                        // aaaand it doesnt work xd
                        .ExtendedBounds(eb => eb.Min(100).Max(500)))
                )));

        var histograms = result.Aggregations?
            .GetHistogram("histogram")?.Buckets
            .Select(x => new { x.Key, x.DocCount });

        return Ok(new { histograms });
    }

    /// <summary>
    /// Finds documents with a field that 1) missing a value 2) missing at all from the schema or was added later.
    /// NullValue() value (from index mapping) is taken into account when aggregating such documents.
    /// </summary>
    [HttpGet]
    [Route("missing")]
    public async Task<IActionResult> Missing()
    {
        var result = await client.SearchAsync<Order>(x => x
            .Index(OrdersIndex)
            .Aggregations(a => a

                // aggregate documents with a missing field (read summary)
                .Add("missing", ag => ag
                    .Missing(m => m.Field("missing_field"))

                    // and find it's total sum or whatever
                    .Aggregations(innerA => innerA
                        .Add("sum", innerAg => innerAg
                            .Sum(s => s.Field(f => f.Total))))
                )));

        var missing = result.Aggregations?.GetMissing("missing")?.DocCount;

        return Ok(new { missing });
    }

    [HttpGet]
    [Route("nested-objects")]
    public async Task<IActionResult> NestedObjects()
    {
        var result = await client.SearchAsync<Order>(x => x
            .Index(OrdersIndex)
            .Size(0)
            .Aggregations(a => a
                .Add("items", ag => ag
                    .Nested(n => n
                        .Path(f => f.Items))
                    .Aggregations(innerA => innerA
                        .Add("max_price", innerAg => innerAg
                            .Max(m => m
                                .Field(f => f.Items.First().Price)))))
            ));

        var totalCount = result.Aggregations?.GetNested("items")?.DocCount;
        var maxPrice = result.Aggregations?.GetNested("items")?
            .Aggregations.GetMax("max_price")?.Value;

        return Ok(new { totalCount, maxPrice });
    }

    [HttpDelete]
    [Route("purge")]
    public async Task<IActionResult> Purge()
    {
        var result = await client.Indices.DeleteAsync<Order>(OrdersIndex);

        return Ok(result);
    }
}

public class Order
{
    public required int Id { get; init; }
    public required DateTime PurchasedAt { get; init; }
    public required string Status { get; init; }
    public required string Channel { get; init; }
    public double Total { get; init; }
    public required Manager Manager { get; init; }
    public required List<LineItem> Items { get; init; } = [];
}

public class Manager
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}

public class LineItem
{
    public required int ProductId { get; init; }
    public required int Quantity { get; init; }
    public required double Price { get; init; }
}