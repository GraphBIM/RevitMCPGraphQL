namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class CategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BuiltInCategory { get; set; }
    public string? CategoryType { get; set; }
    public bool IsCuttable { get; set; }
    public bool AllowsBoundParameters { get; set; }
    public bool IsTagCategory { get; set; }
    public bool CanAddSubcategory { get; set; }
}