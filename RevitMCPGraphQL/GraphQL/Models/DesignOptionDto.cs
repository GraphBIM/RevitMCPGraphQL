namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class DesignOptionDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool? IsPrimary { get; set; }
}
