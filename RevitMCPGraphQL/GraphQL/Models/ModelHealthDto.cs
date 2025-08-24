namespace RevitMCPGraphQL.GraphQL.Models;

internal sealed class ModelHealthDto
{
    public string? DocumentTitle { get; set; }
    public int Warnings { get; set; }
    public int ElementsTotal { get; set; }
    public int RoomsTotal { get; set; }
    public int RoomsUnplaced { get; set; }
    public int RoomsPlaced { get; set; }
    public int AreasTotal { get; set; }
    public int AreasUnplaced { get; set; }
    public int Levels { get; set; }
    public int Phases { get; set; }
    public int ViewsNotOnSheet { get; set; }
    public int ViewsOnSheet { get; set; }
    public int ViewsTotal { get; set; }
    public int ViewTemplatesUsed { get; set; }
    public int ImportInstances { get; set; }
    public int LinkedImports { get; set; }
    public int RevitLinksTotal { get; set; }
    public int RevitLinksLoaded { get; set; }
    public int RevitLinksUnloaded { get; set; }
    public int InPlaceFamilies { get; set; }
    public int Groups { get; set; }
    public int DesignOptions { get; set; }
    public int DesignOptionSets { get; set; }
    public int Worksets { get; set; }
    public bool Workshared { get; set; }
}
