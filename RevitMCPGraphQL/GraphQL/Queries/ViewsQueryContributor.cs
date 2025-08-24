using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ViewsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<ViewGraphType>>("views")
            .Arguments(new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "viewType" },
                new QueryArgument<BooleanGraphType> { Name = "includeTemplates" },
                new QueryArgument<IntGraphType> { Name = "limit" },
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx =>
            {
                var viewType = ctx.GetArgument<string>("viewType");
                var includeTemplates = ctx.GetArgument<bool?>("includeTemplates") ?? false;
                var limit = ctx.GetArgument<int?>("limit");
                var documentId = ctx.GetArgument<long?>("documentId");

                return RevitDispatcher.Invoke(() =>
                {
                    var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                    if (doc == null) return new List<ViewDto>();

                    var views = new FilteredElementCollector(doc)
                        .OfClass(typeof(View))
                        .Cast<View>()
                        .Where(v => includeTemplates || !v.IsTemplate)
                        .Select(v => new ViewDto
                        {
                            Id = v.Id?.Value ?? 0,
                            Name = v.Name,
                            ViewType = v.ViewType.ToString(),
                            IsTemplate = v.IsTemplate,
                            Parameters = v.Parameters
                                .Cast<Parameter>()
                                .Where(p => p?.Definition != null)
                                .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = p.AsValueString() })
                                .ToList()
                        });

                    if (!string.IsNullOrEmpty(viewType))
                        views = views.Where(v => string.Equals(v.ViewType, viewType, StringComparison.OrdinalIgnoreCase));

                    var list = views.OrderBy(v => v.Name).ToList();
                    if (limit.HasValue) list = list.Take(limit.Value).ToList();
                    return list;
                });
            });
    }
}
