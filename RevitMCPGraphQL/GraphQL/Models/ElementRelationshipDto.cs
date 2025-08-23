namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class ElementRelationshipDto
{
    public long ElementId { get; set; }
    public long? SuperComponentId { get; set; }
    public long? HostId { get; set; }
    public List<long> DependentIds { get; set; } = new();
    public List<long> JoinedIds { get; set; } = new();
}
