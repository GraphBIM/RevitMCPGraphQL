namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class ElementDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public long? TypeId { get; set; }
    
    public List<ParameterDto> Parameters { get; set; } = new();
}