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
        Field(x => x.ElementsTotal);
        Field(x => x.RoomsTotal);
        Field(x => x.RoomsUnplaced);
        Field(x => x.RoomsPlaced);
        Field(x => x.AreasTotal);
        Field(x => x.AreasUnplaced);
        Field(x => x.Levels);
        Field(x => x.Phases);
        Field(x => x.ViewsNotOnSheet);
        Field(x => x.ViewsOnSheet);
        Field(x => x.ViewsTotal);
        Field(x => x.ViewTemplatesUsed);
        Field(x => x.ImportInstances);
        Field(x => x.LinkedImports);
        Field(x => x.RevitLinksTotal);
        Field(x => x.RevitLinksLoaded);
        Field(x => x.RevitLinksUnloaded);
        Field(x => x.InPlaceFamilies);
        Field(x => x.Groups);
        Field(x => x.DesignOptions);
        Field(x => x.DesignOptionSets);
        Field(x => x.Worksets);
        Field(x => x.Workshared);
    }
}