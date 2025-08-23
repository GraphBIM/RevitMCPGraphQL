using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ProjectLocationType : ObjectGraphType<ProjectLocationDto>
{
    public ProjectLocationType()
    {
        Field(x => x.SiteName, nullable: true);
        Field(x => x.Latitude, nullable: true);
        Field(x => x.Longitude, nullable: true);
        Field(x => x.Elevation, nullable: true);
    }
}
