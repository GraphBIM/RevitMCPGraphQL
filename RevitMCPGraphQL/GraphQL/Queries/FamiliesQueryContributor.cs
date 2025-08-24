using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class FamiliesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
    query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.FamilyType>>("families")
            .Arguments(new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "categoryName" },
                new QueryArgument<IntGraphType> { Name = "limit" },
                       new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx =>
            {
                var categoryName = ctx.GetArgument<string>("categoryName");
                var limit = ctx.GetArgument<int?>("limit");
                       var documentId = ctx.GetArgument<long?>("documentId");
                return RevitDispatcher.Invoke(() =>
                {
                           var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                    if (doc == null) return new List<FamilyDto>();

                    IEnumerable<Family> fams = new FilteredElementCollector(doc)
                        .OfClass(typeof(Family))
                        .Cast<Family>();

                    if (!string.IsNullOrEmpty(categoryName))
                        fams = fams.Where(f => f.FamilyCategory != null && f.FamilyCategory.Name == categoryName);

                    var list = fams
                        .Select(f => new FamilyDto
                        {
                            Id = f.Id?.Value ?? 0,
                            Name = f.Name,
                            CategoryName = f.FamilyCategory?.Name
                        })
                        .OrderBy(f => f.CategoryName)
                        .ThenBy(f => f.Name)
                        .ToList();

                    if (limit.HasValue) list = list.Take(limit.Value).ToList();
                    return list;
                });
            });
    }
}
