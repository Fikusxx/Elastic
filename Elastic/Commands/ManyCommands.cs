namespace Elastic.Commands;

public class ManyCommands
{
    private class Get
    {
        // GET /orders/_search
        // {
        //     "query": {
        //         "match_all": { } 
        //     }
        // }
        
        // GET /orders/_search
        // "query": {
        //     "term": {
        //         "title": {
        //             "value": "ori"
        //         }
        //     }
        // }
    }
    
    private class Update
    {
        // POST /orders/_update_by_query --updates all documents stock 
        // {
        //     "conflicts": "proceed", 
        //     "script": {
        //         "source": "ctx._source.stock--"
        //     },
        //     "query": {
        //         "match_all": { }
        //     }
        // }
    }

    private class Delete
    {
        // POST /orders/_delete_by_query
        // {
        //     "conflicts": "proceed",
        //     "query": {
        //         "match_all": {}
        //     }
        // }
    }
}