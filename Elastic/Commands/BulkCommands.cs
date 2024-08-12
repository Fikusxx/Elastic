namespace Elastic.Commands;

public class BulkCommands
{
    private class Create
    {
        // POST /_bulk -- indexes are specified explicitly, those might be different
        // { "index" : { "_index" : "orders", "_id" : 1 } } -- command
        // { "id" : 1, "stock" : 10 } -- document
        // { "create" : { "_index" : "orders", "_id" : 2 } }
        // { "id" : 2,  "stock" : 20 }
        
        // POST /orders/_bulk -- index specified in the url
        // { "index" : { "_id" : 1 } } -- command
        // { "id" : 1, "stock" : 10 } -- document
        // { "create" : { "_id" : 2 } }
        // { "id" : 2,  "stock" : 20 }
    }

    private class Update
    {
        // POST /_bulk
        // { "update" : { "_index" : "orders", "_id" : 1 } }
        // { "doc" : { id" : 1, "stock" : 777 } }
        
        // POST /orders/_bulk
        // { "update" : { "_id" : 1 } }
        // { "doc" : { id" : 1, "stock" : 777 } }
    }

    private class Delete
    {
        // POST /_bulk
        // { "delete" : { "_index" : "orders", "_id" : 2 } }
        
        // POST /orders/_bulk
        // { "delete" : { "_id" : 2 } }
    }
}