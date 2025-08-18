namespace RevitMCPGraphQL.GraphQL;

// DTOs to avoid exposing Revit types directly
public sealed class CategoryDto {
	public long Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? BuiltInCategory { get; set; }
	public string? CategoryType { get; set; } // Model / Annotation / Analytical etc
	public bool IsCuttable { get; set; }
	public bool AllowsBoundParameters { get; set; }
	public bool IsTagCategory { get; set; }
	public bool CanAddSubcategory { get; set; }
}
public sealed class ElementDto { public long Id { get; set; } public string? Name { get; set; } }
public sealed class RoomDto { public long Id { get; set; } public string? Name { get; set; } public string? Number { get; set; } public double Area { get; set; } }

