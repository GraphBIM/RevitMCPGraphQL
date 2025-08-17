namespace RevitMCPGraphQL.GraphQL;

// DTOs to avoid exposing Revit types directly
public sealed class CategoryDto { public string Name { get; set; } = string.Empty; public long Id { get; set; } }
public sealed class ElementDto { public long Id { get; set; } public string? Name { get; set; } }
public sealed class RoomDto { public long Id { get; set; } public string? Name { get; set; } public string? Number { get; set; } public double Area { get; set; } }
