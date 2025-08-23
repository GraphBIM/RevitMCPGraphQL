using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsByCategoryQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.ElementType>>("elementsByCategory")
            .Arguments(new QueryArguments(
                new QueryArgument<BuiltInCategoryEnum> { Name = "category" },
                new QueryArgument<IntGraphType> { Name = "limit" }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<ElementDto>();
                var bic = ctx.GetArgument<Autodesk.Revit.DB.BuiltInCategory>("category");
                var limit = ctx.GetArgument<int?>("limit");
                var list = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfCategory(bic)
                    .ToElements()
                    .Select(e => new ElementDto
                    {
                        Id = e.Id?.Value ?? 0,
                        Name = e.Name,
                        Parameters = e.Parameters
                            .Cast<Autodesk.Revit.DB.Parameter>()
                            .Where(p => p != null && p.Definition != null)
                            .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = p.AsValueString() })
                            .ToList()
                    })
                    .ToList();
                if (limit.HasValue) list = list.Take(limit.Value).ToList();
                return list;
            }));
    }
}
