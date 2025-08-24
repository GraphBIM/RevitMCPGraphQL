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
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." },
                new QueryArgument<BooleanGraphType> { Name = "isUnit", Description = "If true (default), parameter values include unit symbols; otherwise numeric only.", DefaultValue = true },
                new QueryArgument<BooleanGraphType> { Name = "isIncludeTypeParams", Description = "If true, also include parameters from the element's type.", DefaultValue = false },
                new QueryArgument<ListGraphType<StringGraphType>> { Name = "parameterNames", Description = "Optional: only include parameters whose names are in this list (case-insensitive)." }
            ))
            .Resolve(ctx =>
            {
                var ids = ctx.GetArgument<List<int>>("ids") ?? new List<int>();
                var documentId = ctx.GetArgument<long?>("documentId");
                var isUnit = ctx.GetArgument<bool>("isUnit", true);
                var includeTypeParams = ctx.GetArgument<bool>("isIncludeTypeParams", false);
                var parameterNames = ctx.GetArgument<List<string>>("parameterNames");
                var includeSet = (parameterNames != null && parameterNames.Count > 0)
                    ? new HashSet<string>(parameterNames, StringComparer.OrdinalIgnoreCase)
                    : null;
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
                                    Parameters = BuildCombinedParameters(e, doc, isUnit, includeTypeParams, includeSet)
                                });
                            }
                        }
                        catch { }
                    }
                    return result;
                });
            });
    }

    private static List<ParameterDto> BuildCombinedParameters(Element e, Autodesk.Revit.DB.Document doc, bool isUnit, bool includeTypeParams, HashSet<string>? includeSet)
    {
        var list = new List<ParameterDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in e.Parameters.Cast<Parameter>())
        {
            if (p?.Definition == null) continue;
            var name = p.Definition!.Name;
            if (includeSet != null && !includeSet.Contains(name)) continue;
            if (seen.Add(name))
                list.Add(new ParameterDto { Name = name, Value = ParameterValueFormatter.GetValue(p, doc, isUnit) });
        }

        if (includeTypeParams)
        {
            try
            {
                var typeId = e.GetTypeId();
                if (typeId != null)
                {
                    var typeElem = doc.GetElement(typeId);
                    if (typeElem != null)
                    {
                        foreach (var p in typeElem.Parameters.Cast<Parameter>())
                        {
                            if (p?.Definition == null) continue;
                            var name = p.Definition!.Name;
                            if (includeSet != null && !includeSet.Contains(name)) continue;
                            if (seen.Add(name))
                                list.Add(new ParameterDto { Name = name, Value = ParameterValueFormatter.GetValue(p, doc, isUnit) });
                        }
                    }
                }
            }
            catch { /* ignore type param errors */ }
        }

        return list;
    }
}
