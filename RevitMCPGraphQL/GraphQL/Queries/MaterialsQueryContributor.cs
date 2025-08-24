using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class MaterialsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<MaterialType>>("materials")
            .Arguments(new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "limit" },
                   new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                   var documentId = ctx.GetArgument<long?>("documentId");
                   var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<MaterialDto>();
                var limit = ctx.GetArgument<int?>("limit");

        var list = new FilteredElementCollector(doc)
                    .OfClass(typeof(Material))
                    .Cast<Material>()
                    .Select(m => new MaterialDto
                    {
                        Id = m.Id?.Value ?? 0,
                        Name = m.Name,
                        Class = m.MaterialClass,
            AppearanceAssetName = (m.AppearanceAssetId != null && m.AppearanceAssetId.Value > 0) ? doc.GetElement(m.AppearanceAssetId)?.Name : null
                    })
                    .OrderBy(m => m.Name)
                    .ToList();

                if (limit.HasValue) list = list.Take(limit.Value).ToList();
                return list;
            }));
    }
}
