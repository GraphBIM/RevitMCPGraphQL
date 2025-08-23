using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class GridType : ObjectGraphType<GridDto>
{
    public GridType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
    }
}
