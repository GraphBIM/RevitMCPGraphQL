namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class LevelDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public double Elevation { get; set; }
    public List<ParameterDto> Parameters { get; set; } = new();
}
