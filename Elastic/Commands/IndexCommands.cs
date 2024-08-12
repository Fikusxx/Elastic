namespace Elastic.Commands;

/// <summary>
/// Text fields are stored within dedicated inverted indexes
/// Inverted index is a mapping between terms (tokens, words) and what documents contain them
/// Other data types are stored as BKD trees (numeric, geo, date, etc)
/// </summary>
public class IndexCommands
{
    // DELETE /orders
    
    // GET /complex-index/_mapping --mappings for an index

    // PUT /orders --creates an index
    // {
    //     "settings": {
    //         "number_of_shards": 1,
    //         "number_of_replicas": 0
    //     }
    // }
    
    // PUT /complex-index --creates an index
    // {
    //     "mappings": {
    //         "properties": {
    //             "id" : {"type": "integer"},
    //             "text" :{"type": "text"},
    //             "person" : {
    //                 "properties": {
    //                     "name" : {"type": "text"}
    //                 }
    //             }
    //         }
    //     }
    // }
    
    // PUT /complex-index/_mapping --add property to an index
    // {
    //     "properties": {
    //         "created_at" : {"type": "date"}
    //      }
    // }
}