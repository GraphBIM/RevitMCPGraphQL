using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class DesignOptionType : ObjectGraphType<DesignOptionDto>
{
    public DesignOptionType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.IsPrimary, nullable: true);
    }
}
