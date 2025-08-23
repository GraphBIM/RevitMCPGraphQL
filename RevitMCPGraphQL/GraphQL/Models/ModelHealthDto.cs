namespace RevitMCPGraphQL.GraphQL.Models;

internal sealed class ModelHealthDto
{
    public string? DocumentTitle { get; set; }
    public int Warnings { get; set; }
    public int RoomsTotal { get; set; }
    public int RoomsUnplaced { get; set; }
    public int ViewsNotOnSheet { get; set; }
    public int ImportInstances { get; set; }
    public int LinkedImports { get; set; }
    public int InPlaceFamilies { get; set; }
    public int Groups { get; set; }
    public int DesignOptions { get; set; }
    public int Worksets { get; set; }
}
