namespace Elastic.Commands;

public class ReindexCommands
{
    // POST /_reindex --reindex existing document into a new index
    // {
    //     "source": {
    //         "index" : "orders",
    //         "_source" : [ "fieldName1", "fieldName2" ], -- fields to reindex
    //         "query": {
    //             "match_all": { } -- documents to reindex
    //         },
    //     },
    //     "dest": {
    //         "index": "orders_new"
    //     },
    //     "script": { -- any logic goes here, i.e changing data types, renaming, etc
    //         "source": """
    //                     if(ctx._source.id != null) {
    //                       ctx._source.id = ctx._source.id.ToString();
    //                     }
    //                   """
    //     }
    // }
}