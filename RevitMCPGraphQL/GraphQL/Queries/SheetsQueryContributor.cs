using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;
using GraphQL;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class SheetsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<SheetType>>("sheets")
                .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                    var documentId = ctx.GetArgument<long?>("documentId");
                    var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<SheetDto>();
                var sheets = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .Select(s => new SheetDto
                    {
                        Id = s.Id?.Value ?? 0,
                        SheetNumber = s.SheetNumber,
                        Name = s.Name,
                        PlacedViews = s.GetAllPlacedViews().Select(id => doc.GetElement(id)?.Name ?? $"{id.Value}").ToList()
                    })
                    .OrderBy(s => s.SheetNumber)
                    .ToList();
                return sheets;
            }));
    }
}
