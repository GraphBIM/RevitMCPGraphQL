using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class MaterialType : ObjectGraphType<MaterialDto>
{
    public MaterialType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.Class, nullable: true).Description("Material class");
        Field(x => x.AppearanceAssetName, nullable: true);
    }
}
