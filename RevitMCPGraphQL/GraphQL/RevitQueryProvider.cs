using GraphQL;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL;

internal class RevitQueryProvider
{
    private readonly Func<Document?> _getDoc;
    public RevitQueryProvider(Func<Document?> getDoc) { _getDoc = getDoc; }

    public IObjectGraphType GetQuery()
    {
        var query = new ObjectGraphType();

        // Simple health check query
        query.Field<StringGraphType>("health").Resolve(_ => "ok");

        query.Field<ListGraphType<CategoryType>>("categories")
            .Arguments(new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "limit" }
            ))
            .Resolve(context =>
        {
            return RevitDispatcher.Invoke(() =>
            {
                var doc = _getDoc();
                if (doc == null) return new List<CategoryDto>();
                // categories of current document instnace document 
                var categories = doc.Settings.Categories
                    .Cast<Category>()
                    .Where(c => c != null)
                    .Select(c =>
                    {
                        string? bicName = null;
                        try
                        {
                            var intId = c.Id.Value;
                            if (System.Enum.IsDefined(typeof(BuiltInCategory), intId))
                                bicName = ((BuiltInCategory)intId).ToString();
                        }
                        catch { }
                        string? typeName = null;
                        try { typeName = c.CategoryType.ToString(); } catch { }
                        bool isCuttable = false; try { isCuttable = c.IsCuttable; } catch { }
                        bool allowsBound = false; try { allowsBound = c.AllowsBoundParameters; } catch { }
                        bool isTag = false; try { isTag = c.IsTagCategory; } catch { }
                        bool canAddSub = false; try { canAddSub = c.CanAddSubcategory; } catch { }
                        return new CategoryDto
                        {
                            Id = c.Id!.Value,
                            Name = c.Name,
                            BuiltInCategory = bicName,
                            CategoryType = typeName,
                            IsCuttable = isCuttable,
                            AllowsBoundParameters = allowsBound,
                            IsTagCategory = isTag,
                            CanAddSubcategory = canAddSub
                        };
                    })
                    .OrderBy(c => c.Name)
                    .ToList();
                var limit = context.GetArgument<int?>("limit");
                if (limit.HasValue)
                    categories = categories.Take(limit.Value).ToList();

                return categories;
            });
        });

        query.Field<ListGraphType<ElementType>>("elements")
            .Arguments(new QueryArguments(new QueryArgument<StringGraphType> { Name = "categoryName" }))
            .Resolve(context =>
            {
                var categoryName = context.GetArgument<string>("categoryName");
                return RevitDispatcher.Invoke(() =>
                {
                    var doc = _getDoc();
                    if (doc == null) return new List<ElementDto>();
                    var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        var category = doc.Settings.Categories.Cast<Category>().FirstOrDefault(c => c.Name == categoryName);
                        if (category != null)
                        {
                            try { collector = collector.OfCategory((BuiltInCategory)category.Id.IntegerValue); } catch { }
                        }
                    }
                    return collector.ToElements()
                        .Cast<Element>()
                        .Select(e => new ElementDto { Id = e.Id?.Value ?? 0, Name = e.Name })
                        .ToList();
                });
            });

        query.Field<ListGraphType<RoomType>>("rooms").Resolve(context =>
        {
            return RevitDispatcher.Invoke(() =>
            {
                var doc = _getDoc();
                if (doc == null) return new List<RoomDto>();
                return new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<SpatialElement>()
                    .Select(r => new RoomDto {
                        Id = r.Id?.Value ?? 0,
                        Name = r.Name,
                        Number = r.Number,
                        Area = r.Area
                    })
                    .ToList();
            });
        });

        return query;
    }
}
