using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class LinkType : ObjectGraphType<LinkDto>
{
    public LinkType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.Path, nullable: true);
        Field(x => x.IsLoaded, nullable: true);
    }
}
