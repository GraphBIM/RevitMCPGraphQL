using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class BuiltInCategoryEnum : EnumerationGraphType<Autodesk.Revit.DB.BuiltInCategory>
{
    public BuiltInCategoryEnum()
    {
        Name = "BuiltInCategory";
        Description = "Safe enum for Revit BuiltInCategory";
    }
}
