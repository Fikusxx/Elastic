namespace Elastic.Commands;

/// <summary>
/// Text fields are stored within dedicated inverted indexes
/// Inverted index is a mapping between terms (tokens, words) and what documents contain them
/// Other data types are stored as BKD trees (numeric, geo, date, etc)
/// Also: existing field mappings cannot be updated/deleted. Solution is to reindex into a new index.
/// </summary>
public class IndexCommands
{
    // DELETE /orders --delete an index
    
    // GET /complex-index --all data for an index
    // GET /complex-index/_mapping --mappings for an index
    // GET /complex-index/_settings --settings for an index
    
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
    //     },
    //      "settings": {
    //         "number_of_shards": 1,
    //         "number_of_replicas": 0
    //     }
    // }
    
    // PUT /complex-index/_mapping --add property to an index
    // {
    //     "properties": {
    //         "created_at" : {"type": "date"}
    //      }
    // }

    #region Dynamic

    // PUT /complex-index --creates an index
    // {
    //     "mappings": {
    //         "dynamic": "strict", -- disables dynamic mapping for new fields (FALSE: no inverted/bkd indexes created for them, STRICT: error on insert)
    //         "properties": {
    //             "id" : {"type": "integer"},
    //             "person" : {
    //                 "dynamic": true, -- overrides high level dynamic mapping
    //                 "properties": {
    //                     "name" : {"type": "text"}
    //                 }
    //             }
    //         }
    //     }
    // }

    #endregion

    #region Alias

    // search queries will work for either of field (or alias) names
    // PUT /complex-index/_mapping --add alias to a field
    // {
    //     "properties": {
    //         "comment" : {"type": "alias", "path": "description"}
    //      }
    // }
    
    #endregion

    #region Multifield

    // PUT /multi_field
    // {
    //     "mappings": {
    //         "properties": {
    //             "comment" : {
    //                 "type": "text",
    //                 "fields": {
    //                     "keyword": {
    //                         "type" : "keyword"
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }
    //
    // GET /multi_field/_search
    // {
    //     "query": {
    //         "match_all": {
    //             "comment" : "value" -- text search
    //         }
    //     },
    //     "query": {
    //         "term": {
    //             "comment.keyword": "Exact value" -- exact value search
    //         }
    //     }
    // }

    #endregion
}