namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class MaterialDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Class { get; set; }
    public string? AppearanceAssetName { get; set; }
}
