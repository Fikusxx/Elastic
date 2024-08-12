namespace Elastic.Commands;

public class AnalyzeCommands
{
    // POST /_analyze
    // {
    //     "text": ["Hello! Im a retard 123 :)"],
    //     "analyzer": "standard" -- default
    // }
    
    // POST /_analyze -- standard analyzer converted to this
    // {
    //     "text": ["Hello! Im a retard 123 :)"],
    //     "char_filter": [],
    //     "tokenizer": "standard",
    //     "filter": ["lowercase"]
    // }

}