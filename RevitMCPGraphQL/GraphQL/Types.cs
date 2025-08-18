using GraphQL.Types;
using RevitMCPGraphQL.GraphQL;

namespace RevitMCPGraphQL;

public class CategoryType : ObjectGraphType<CategoryDto>
{
    public CategoryType()
    {
    Field(x => x.Id).Description("Category ID");
    Field(x => x.Name).Description("Category name");
    Field(x => x.BuiltInCategory, nullable: true).Description("BuiltInCategory enum name if resolvable");
    Field(x => x.CategoryType, nullable: true).Description("Category classification");
    Field(x => x.IsCuttable).Description("Is cuttable in views");
    Field(x => x.AllowsBoundParameters).Description("Allows bound parameters");
    Field(x => x.IsTagCategory).Description("Is a tag category");
    Field(x => x.CanAddSubcategory).Description("Can add subcategories");
    }
}

public class ElementType : ObjectGraphType<ElementDto>
{
    public ElementType()
    {
        Field(x => x.Id).Description("Element ID");
        Field(x => x.Name, nullable: true).Description("Element name");
    }
}

public class RoomType : ObjectGraphType<RoomDto>
{
    public RoomType()
    {
        Field(x => x.Id).Description("Room ID");
        Field(x => x.Name, nullable: true).Description("Room name");
        Field(x => x.Number, nullable: true).Description("Room number");
        Field(x => x.Area).Description("Room area");
    }
}

public class ParameterType : ObjectGraphType<Autodesk.Revit.DB.Parameter>
{
    public ParameterType()
    {
        Field<StringGraphType>("name").Resolve(context => context.Source.Definition.Name);
        Field<StringGraphType>("value").Resolve(context => context.Source.AsValueString());
    }
}

