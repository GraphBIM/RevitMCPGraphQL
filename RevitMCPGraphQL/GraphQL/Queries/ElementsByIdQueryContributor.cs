using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsByIdQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
    query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.ElementType>>("elementsById")
            .Arguments(new QueryArguments(
                new QueryArgument<ListGraphType<IdGraphType>> { Name = "ids" },
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx =>
            {
                var ids = ctx.GetArgument<List<int>>("ids") ?? new List<int>();
                var documentId = ctx.GetArgument<long?>("documentId");
                return RevitDispatcher.Invoke(() =>
                {
                    var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                    if (doc == null || ids.Count == 0) return new List<ElementDto>();

                    var idSet = new HashSet<int>(ids);
                    var result = new List<ElementDto>();
                    foreach (var id in idSet)
                    {
                        try
                        {
                            var e = doc.GetElement(new ElementId((long)id));
                            if (e != null)
                            {
                                result.Add(new ElementDto
                                {
                                    Id = e.Id?.Value ?? 0,
                                    TypeId = e.GetTypeId()?.Value,
                                    Name = e.Name,
                                    Parameters = e.Parameters
                                        .Cast<Parameter>()
                                        .Where(p => p?.Definition != null)
                                        .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = p.AsValueString() })
                                        .ToList()
                                });
                            }
                        }
                        catch { }
                    }
                    return result;
                });
            });
    }
}
