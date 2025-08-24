namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class RoomDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Number { get; set; }
    public List<ParameterDto> Parameters { get; set; } = new();
    public BoundingBoxDto? BBox { get; set; }
    public List<List<double[]>> Boundaries { get; set; } = new(); // Each inner list is a loop of [x,y,z] points
    public List<long> ElementsInside { get; set; } = new();
}