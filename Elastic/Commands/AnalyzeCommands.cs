namespace Elastic.Commands;

public class AnalyzeCommands
{
    // GET /my_index/_search -- explains query 
    // {
    //     "explain": true, 
    //     "query": {
    //         "match_all": {}
    //     }
    // }
    
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

    #region Standard

    // lowercase and splits text at word boundaries and removes punctuation, also supports removing stop words
    // "Hello! My name is Tom?" => ["hello", "my", "name", "is", "tom"]

    #endregion

    #region Simple && Stop

    // lowercase and splits text when encountering anything else than letters
    // "Hello! Isn't this Tom's dog?" => [ "hello", "isn", "t", "this", "tom", "s", "dog" ]
    
    // stop analyzer is just like simple, but also supports removing stop words

    #endregion

    #region Whitespace

    // just splits text when space is encountered
    // "Hello! This is Tom." => [ "Hello!", "This", "is", "Tom." ]

    #endregion

    #region Keyword

    // leaves text as is, used for exact search
    // "Hello!" => [ "Hello!" ]

    #endregion

    #region  Languange

    // specific for a language, supports russian/english for example

    #endregion
}