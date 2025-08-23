namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class FamilyInstanceDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? FamilyName { get; set; }
    public string? TypeName { get; set; }
    public string? CategoryName { get; set; }
    public long? LevelId { get; set; }
    public List<ParameterDto> Parameters { get; set; } = new();
}
