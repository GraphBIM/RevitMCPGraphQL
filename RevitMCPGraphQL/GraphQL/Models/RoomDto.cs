namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class RoomDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Number { get; set; }
    public List<ParameterDto> Parameters { get; set; } = new();
}