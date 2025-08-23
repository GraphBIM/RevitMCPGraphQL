namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class FamilySymbolDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? FamilyName { get; set; }
    public string? CategoryName { get; set; }
    public List<ParameterDto> Parameters { get; set; } = new();
}
