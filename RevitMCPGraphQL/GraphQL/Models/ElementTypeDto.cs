namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class ElementTypeDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? CategoryName { get; set; }
    public List<ParameterDto> Parameters { get; set; } = new();
}
