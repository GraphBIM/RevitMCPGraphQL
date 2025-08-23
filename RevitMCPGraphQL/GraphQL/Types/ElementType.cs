using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ElementType : ObjectGraphType<ElementDto>
{
    public ElementType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.TypeId, nullable: true);
        Field<ListGraphType<ParameterType>>("parameters")
            .Resolve(context => context.Source.Parameters)
            .Description("List of parameters for the element");
    }
}