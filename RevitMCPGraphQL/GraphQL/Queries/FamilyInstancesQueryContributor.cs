using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class FamilyInstancesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<FamilyInstanceType>>("familyInstances")
            .Arguments(new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "categoryName" },
                new QueryArgument<StringGraphType> { Name = "familyName" },
                new QueryArgument<StringGraphType> { Name = "typeName" },
                new QueryArgument<IntGraphType> { Name = "limit" },
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx =>
            {
                var categoryName = ctx.GetArgument<string>("categoryName");
                var familyName = ctx.GetArgument<string>("familyName");
                var typeName = ctx.GetArgument<string>("typeName");
                var limit = ctx.GetArgument<int?>("limit");
                var documentId = ctx.GetArgument<long?>("documentId");

                return RevitDispatcher.Invoke(() =>
                {
                    var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                    if (doc == null) return new List<FamilyInstanceDto>();

                    IEnumerable<FamilyInstance> inst = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilyInstance))
                        .Cast<FamilyInstance>();

                    if (!string.IsNullOrEmpty(categoryName))
                        inst = inst.Where(i => i.Category != null && i.Category.Name == categoryName);
                    if (!string.IsNullOrEmpty(familyName))
                        inst = inst.Where(i => i.Symbol?.Family?.Name == familyName);
                    if (!string.IsNullOrEmpty(typeName))
                        inst = inst.Where(i => i.Symbol?.Name == typeName);

                    var list = inst.Select(i => new FamilyInstanceDto
                        {
                            Id = i.Id?.Value ?? 0,
                            Name = i.Name,
                            FamilyName = i.Symbol?.Family?.Name,
                            TypeName = i.Symbol?.Name,
                            CategoryName = i.Category?.Name,
                            LevelId = i.LevelId?.Value,
                            Parameters = i.Parameters
                                .Cast<Parameter>()
                                .Where(p => p?.Definition != null)
                                .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = p.AsValueString() })
                                .ToList()
                        })
                        .OrderBy(i => i.FamilyName)
                        .ThenBy(i => i.TypeName)
                        .ThenBy(i => i.Name)
                        .ToList();

                    if (limit.HasValue) list = list.Take(limit.Value).ToList();
                    return list;
                });
            });
    }
}
