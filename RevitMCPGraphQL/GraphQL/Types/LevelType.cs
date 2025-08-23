using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class LevelType : ObjectGraphType<LevelDto>
{
    public LevelType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.Elevation);
        Field<ListGraphType<ParameterType>>("parameters")
            .Resolve(ctx => ctx.Source.Parameters);
    }
}
