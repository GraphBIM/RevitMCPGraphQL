using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class FamilyType : ObjectGraphType<FamilyDto>
{
    public FamilyType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.CategoryName, nullable: true);
    }
}
