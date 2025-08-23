namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class SheetDto
{
    public long Id { get; set; }
    public string? SheetNumber { get; set; }
    public string? Name { get; set; }
    public List<string> PlacedViews { get; set; } = new();
}
