using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class FamilySymbolType : ObjectGraphType<FamilySymbolDto>
{
    public FamilySymbolType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.FamilyName, nullable: true);
        Field(x => x.CategoryName, nullable: true);
        Field<ListGraphType<ParameterType>>("parameters")
            .Resolve(ctx => ctx.Source.Parameters)
            .Description("List of parameters for the family type");
    }
}
