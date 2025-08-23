using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class WorksetType : ObjectGraphType<WorksetDto>
{
    public WorksetType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.Kind, nullable: true);
        Field(x => x.Owner, nullable: true);
        Field(x => x.IsOpen, nullable: true);
    }
}
