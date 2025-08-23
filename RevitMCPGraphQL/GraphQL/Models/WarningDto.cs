namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class WarningDto
{
    public string? Description { get; set; }
    public List<long> ElementIds { get; set; } = new();
}
