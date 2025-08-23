using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class FamilyInstanceType : ObjectGraphType<FamilyInstanceDto>
{
    public FamilyInstanceType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.FamilyName, nullable: true);
        Field(x => x.TypeName, nullable: true);
        Field(x => x.CategoryName, nullable: true);
        Field(x => x.LevelId, nullable: true);
        Field<ListGraphType<ParameterType>>("parameters")
            .Resolve(ctx => ctx.Source.Parameters);
    }
}
