using GraphQL.Types;
using RevitMCPGraphQL.GraphQL;

namespace RevitMCPGraphQL;

public class CategoryType : ObjectGraphType<CategoryDto>
{
    public CategoryType()
    {
        Field(x => x.Name).Description("Category name");
        Field(x => x.Id).Description("Category ID");
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

