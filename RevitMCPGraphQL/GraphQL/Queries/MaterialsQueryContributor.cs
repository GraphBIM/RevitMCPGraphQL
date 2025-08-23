using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class MaterialsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<MaterialType>>("materials")
            .Arguments(new QueryArguments(new QueryArgument<IntGraphType> { Name = "limit" }))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
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
