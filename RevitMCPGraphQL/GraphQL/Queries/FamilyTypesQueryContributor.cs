using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class FamilyTypesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ListGraphType<FamilySymbolType>>("familyTypes")
            .Arguments(new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "categoryName" },
                new QueryArgument<StringGraphType> { Name = "familyName" },
                new QueryArgument<IntGraphType> { Name = "limit" },
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(context =>
            {
                var categoryName = context.GetArgument<string>("categoryName");
                var familyName = context.GetArgument<string>("familyName");
                var limit = context.GetArgument<int?>("limit");
                var requestedId = context.GetArgument<long?>("documentId");

                return RevitDispatcher.Invoke(() =>
                {
                    var hostDoc = getDoc();
                    var doc = DocumentResolver.ResolveDocument(hostDoc, requestedId);
                    if (doc == null) return new List<FamilySymbolDto>();

                    IEnumerable<FamilySymbol> symbols = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>();

                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        symbols = symbols.Where(s => s.Category != null && s.Category.Name == categoryName);
                    }
                    if (!string.IsNullOrEmpty(familyName))
                    {
                        symbols = symbols.Where(s => s.Family != null && s.Family.Name == familyName);
                    }

                    var list = symbols
                        .Select(s => new FamilySymbolDto
                        {
                            Id = s.Id?.Value ?? 0,
                            Name = s.Name,
                            FamilyName = s.Family?.Name,
                            CategoryName = s.Category?.Name,
                            Parameters = s.Parameters
                                .Cast<Parameter>()
                                .Where(p => p != null && p.Definition != null)
                                .Select(p => new ParameterDto
                                {
                                    Name = p.Definition!.Name,
                                    Value = p.AsValueString(),
                                })
                                .ToList()
                        })
                        .OrderBy(s => s.FamilyName)
                        .ThenBy(s => s.Name)
                        .ToList();

                    if (limit.HasValue)
                        list = list.Take(limit.Value).ToList();

                    return list;
                });
            });
    }
}
