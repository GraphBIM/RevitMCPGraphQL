using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class SheetType : ObjectGraphType<SheetDto>
{
    public SheetType()
    {
        Field(x => x.Id);
        Field(x => x.SheetNumber, nullable: true);
        Field(x => x.Name, nullable: true);
        Field<ListGraphType<StringGraphType>>("placedViews")
            .Resolve(ctx => ctx.Source.PlacedViews);
    }
}
