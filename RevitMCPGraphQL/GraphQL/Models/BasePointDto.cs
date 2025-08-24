namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class BasePointDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool IsShared { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}
