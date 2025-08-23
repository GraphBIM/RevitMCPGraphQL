using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

internal sealed class ModelHealthType : ObjectGraphType<ModelHealthDto>
{
    public ModelHealthType()
    {
        Name = "ModelHealth";
        Field(x => x.DocumentTitle, nullable: true);
        Field(x => x.Warnings);
        Field(x => x.RoomsTotal);
        Field(x => x.RoomsUnplaced);
        Field(x => x.ViewsNotOnSheet);
        Field(x => x.ImportInstances);
        Field(x => x.LinkedImports);
        Field(x => x.InPlaceFamilies);
        Field(x => x.Groups);
        Field(x => x.DesignOptions);
        Field(x => x.Worksets);
    }
}
