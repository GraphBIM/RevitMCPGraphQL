using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ProjectInfoType : ObjectGraphType<ProjectInfoDto>
{
    public ProjectInfoType()
    {
        Field(x => x.ProjectName, nullable: true);
        Field(x => x.ProjectNumber, nullable: true);
        Field(x => x.OrganizationName, nullable: true);
        Field(x => x.BuildingName, nullable: true);
    }
}
