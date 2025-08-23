namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class ViewDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? ViewType { get; set; }
    public bool? IsTemplate { get; set; }
    public List<ParameterDto> Parameters { get; set; } = new();
}
