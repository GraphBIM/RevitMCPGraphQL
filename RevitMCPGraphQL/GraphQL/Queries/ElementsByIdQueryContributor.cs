using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsByIdQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
    query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.ElementType>>("elementsById")
            .Arguments(new QueryArguments(new QueryArgument<ListGraphType<IntGraphType>> { Name = "ids" }))
            .Resolve(ctx =>
            {
                var ids = ctx.GetArgument<List<int>>("ids") ?? new List<int>();
                return RevitDispatcher.Invoke(() =>
                {
                    var doc = getDoc();
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
