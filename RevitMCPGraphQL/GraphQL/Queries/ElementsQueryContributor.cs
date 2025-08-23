using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using ElementType = RevitMCPGraphQL.GraphQL.Types.ElementType;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<ElementType>>("elements")
            .Arguments(new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "categoryName" },
                new QueryArgument<IntGraphType> { Name = "limit" }
            ))
            .Resolve(context =>
            {
                var categoryName = context.GetArgument<string>("categoryName");
                var limit = context.GetArgument<int?>("limit");
                return RevitDispatcher.Invoke(() =>
                {
                    var doc = getDoc();
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
                            catch { }
                        }
                    }

                    var list = collector.ToElements()
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

                    if (limit.HasValue) list = list.Take(limit.Value).ToList();
                    return list;
                });
            });
    }
}
