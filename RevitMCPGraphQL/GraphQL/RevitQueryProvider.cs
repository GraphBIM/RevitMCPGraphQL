using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using CategoryType = RevitMCPGraphQL.GraphQL.Types.CategoryType;
using DocumentType = Autodesk.Revit.DB.DocumentType;
using ElementType = RevitMCPGraphQL.GraphQL.Types.ElementType;

namespace RevitMCPGraphQL.GraphQL;

internal class RevitQueryProvider
{
    private readonly Func<Document?> _getDoc;

    public RevitQueryProvider(Func<Document?> getDoc)
    {
        _getDoc = getDoc;
    }

    public IObjectGraphType GetQuery()
    {
        var query = new ObjectGraphType();

        // Simple health check query
        query.Field<StringGraphType>("health").Resolve(_ => "ok");

        // Document info
        query.Field<RevitMCPGraphQL.GraphQL.Types.DocumentType>("document").Resolve(_ =>
        {
            return RevitDispatcher.Invoke(() =>
            {
                var doc = _getDoc();
                if (doc == null)
                    return new DocumentDto { Title = string.Empty, PathName = null, IsFamilyDocument = false };
                return new DocumentDto
                {
                    Title = doc.Title ?? string.Empty,
                    PathName = doc.PathName,
                    IsFamilyDocument = doc.IsFamilyDocument,
                };
            });
        });

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
                            catch
                            {
                            }

                            string? typeName = null;
                            try
                            {
                                typeName = c.CategoryType.ToString();
                            }
                            catch
                            {
                            }

                            bool isCuttable = false;
                            try
                            {
                                isCuttable = c.IsCuttable;
                            }
                            catch
                            {
                            }

                            bool allowsBound = false;
                            try
                            {
                                allowsBound = c.AllowsBoundParameters;
                            }
                            catch
                            {
                            }

                            bool isTag = false;
                            try
                            {
                                isTag = c.IsTagCategory;
                            }
                            catch
                            {
                            }

                            bool canAddSub = false;
                            try
                            {
                                canAddSub = c.CanAddSubcategory;
                            }
                            catch
                            {
                            }

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
                        var category = doc.Settings.Categories.Cast<Category>()
                            .FirstOrDefault(c => c.Name == categoryName);
                        if (category != null)
                        {
                            try
                            {
                                collector = collector.OfCategory((BuiltInCategory)category.Id.Value);
                            }
                            catch
                            {
                            }
                        }
                    }

                    return collector.ToElements()
                        .Cast<Element>()
                        .Select(e => new ElementDto
                        {
                            Id = e.Id?.Value ?? 0,
                            Name = e.Name,
                            Parameters = e.Parameters
                                .Cast<Parameter>()
                                .Where(p => p != null && p.Definition != null)
                                .Select(p => new ParameterDto
                                {
                                    Name = p.Definition!.Name,
                                    Value = p.AsValueString(),
                                })
                                .ToList(),
                        })
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
                    .Select(r => new RoomDto
                    {
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