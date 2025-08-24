using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsByCategoryQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.ElementType>>("elementsByCategory")
        .Arguments(new QueryArguments(
                new QueryArgument<BuiltInCategoryEnum> { Name = "category" },
                new QueryArgument<IntGraphType> { Name = "limit" },
            new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." },
            new QueryArgument<BooleanGraphType> { Name = "isUnit", Description = "If true (default), parameter values include unit symbols; otherwise numeric only.", DefaultValue = true }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                    var documentId = ctx.GetArgument<long?>("documentId");
            var isUnit = ctx.GetArgument<bool>("isUnit", true);
                    var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<ElementDto>();
                var bic = ctx.GetArgument<Autodesk.Revit.DB.BuiltInCategory?>("category", null);
                var limit = ctx.GetArgument<int?>("limit");
                var collector = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType();
                if (bic.HasValue)
                    collector.OfCategory(bic.Value);
                var list = collector
                    .ToElements()
                    .Select(e => new ElementDto
                    {
                        Id = e.Id?.Value ?? 0,
                        TypeId = e.GetTypeId()?.Value,
                        Name = e.Name,
                        Parameters = e.Parameters
                            .Cast<Autodesk.Revit.DB.Parameter>()
                            .Where(p => p != null && p.Definition != null)
                            .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = ParameterValueFormatter.GetValue(p, doc, isUnit) })
                            .ToList()
                    })
                    .ToList();
                if (limit.HasValue) list = list.Take(limit.Value).ToList();
                return list;
            }));
    }
}
