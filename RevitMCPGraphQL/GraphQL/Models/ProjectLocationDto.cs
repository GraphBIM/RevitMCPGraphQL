namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class ProjectLocationDto
{
    public string? SiteName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Elevation { get; set; }
}
