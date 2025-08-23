using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ViewGraphType : ObjectGraphType<ViewDto>
{
    public ViewGraphType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.ViewType, nullable: true);
        Field(x => x.IsTemplate, nullable: true);
        Field<ListGraphType<ParameterType>>("parameters")
            .Resolve(ctx => ctx.Source.Parameters);
    }
}
