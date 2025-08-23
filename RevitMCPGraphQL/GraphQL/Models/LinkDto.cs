namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class LinkDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
    public bool? IsLoaded { get; set; }
}
