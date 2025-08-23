namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class UnitDto
{
    public string TypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
}
