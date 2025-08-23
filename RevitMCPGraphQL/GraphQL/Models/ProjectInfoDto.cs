namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class ProjectInfoDto
{
    public string? ProjectName { get; set; }
    public string? ProjectNumber { get; set; }
    public string? OrganizationName { get; set; }
    public string? BuildingName { get; set; }
}
