using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

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