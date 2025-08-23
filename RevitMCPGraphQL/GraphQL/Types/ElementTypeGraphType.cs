using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ElementTypeGraphType : ObjectGraphType<ElementTypeDto>
{
    public ElementTypeGraphType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.CategoryName, nullable: true);
        Field<ListGraphType<ParameterType>>("parameters")
            .Resolve(ctx => ctx.Source.Parameters);
    }
}
