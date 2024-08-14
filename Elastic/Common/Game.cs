using System.Text.Json.Serialization;

namespace Elastic.Common;

public sealed class Game
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required int Price { get; set; }
}