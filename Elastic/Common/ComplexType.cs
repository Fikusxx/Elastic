namespace Elastic.Common;

public sealed class ComplexType
{
    public required int Id { get; set; }
    public required string Description { get; set; }
    public required List<string> Tags { get; set; }
    public required  ComplexTypeNum Enum { get; set; }
    public required int Int { get; set; }
    public required float Float { get; set; }
    public required double Double { get; set; }
    public required short Short { get; set; }
    public required DateTime DateTime { get; set; }
    public required DateOnly DateOnly { get; set; }
    public required InnerOne InnerOne { get; set; }
    public required List<InnerTwo> InnerTwos { get; set; }
    public required bool Bool { get; set; }
    public required double[] GeoPoint { get; set; }
    public string? IpAddress { get; set; }
}

public sealed class InnerOne
{
    public required string Value { get; set; }
    public required List<int> Values { get; set; }
}

public sealed class InnerTwo
{
    public required string Value { get; set; } 
}

public enum ComplexTypeNum
{
    One,
    Two
}