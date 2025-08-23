namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class WorksetDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Kind { get; set; }
    public string? Owner { get; set; }
    public bool? IsOpen { get; set; }
}
