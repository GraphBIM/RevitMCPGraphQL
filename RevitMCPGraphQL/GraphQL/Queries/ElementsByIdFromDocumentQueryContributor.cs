using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsByIdFromDocumentQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.ElementType>>("elementsByIdFromDocument")
            .Description("Get elements by ids from current document when documentId is null; otherwise from the specified loaded link document (RevitLinkInstance element id).")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<IdGraphType>>>> { Name = "ids" },
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var host = getDoc();
                var ids = ctx.GetArgument<List<long>>("ids") ?? new List<long>();
                var documentId = ctx.GetArgument<long?>("documentId");
                if (host == null || ids.Count == 0) return new List<ElementDto>();

                var target = DocumentResolver.ResolveDocument(host, documentId);
                if (target == null) return new List<ElementDto>();

                var idSet = new HashSet<long>(ids);
                var result = new List<ElementDto>();
                foreach (var id in idSet)
                {
                    try
                    {
                        var e = target.GetElement(new Autodesk.Revit.DB.ElementId(id));
                        if (e != null)
                        {
                            result.Add(new ElementDto
                            {
                                Id = e.Id?.Value ?? 0,
                                TypeId = e.GetTypeId()?.Value,
                                Name = e.Name,
                                Parameters = e.Parameters
                                    .Cast<Autodesk.Revit.DB.Parameter>()
                                    .Where(p => p?.Definition != null)
                                    .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = p.AsValueString() })
                                    .ToList()
                            });
                        }
                    }
                    catch { }
                }
                return result;
            }));
    }
}
