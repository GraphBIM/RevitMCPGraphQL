using Autodesk.Revit.DB;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class SheetsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<SheetType>>("sheets")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
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
