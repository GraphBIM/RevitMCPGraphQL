using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class UnitGraphType : ObjectGraphType<UnitDto>
{
    public UnitGraphType()
    {
        Field(x => x.TypeId);
        Field(x => x.Name);
        Field(x => x.Symbol, nullable: true);
    }
}
