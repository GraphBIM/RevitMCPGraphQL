using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class WarningType : ObjectGraphType<WarningDto>
{
    public WarningType()
    {
        Field(x => x.Description, nullable: true);
        Field<ListGraphType<IntGraphType>>("elementIds")
            .Resolve(ctx => ctx.Source.ElementIds);
    }
}
