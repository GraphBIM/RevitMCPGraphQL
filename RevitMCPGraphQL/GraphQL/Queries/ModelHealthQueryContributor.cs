using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using System.Linq;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ModelHealthQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ModelHealthType>("modelHealth")
            .Description("Quick model health summary for the active document.")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new ModelHealthDto();

                // Warnings
                var warnings = 0;
                try { warnings = doc.GetWarnings()?.Count ?? 0; } catch { warnings = 0; }

                // Rooms
                var rooms = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements();

                var roomsTotal = rooms.Count;
                var roomsUnplaced = rooms
                    .OfType<Room>()
                    .Count(r => r.Location == null);

                // Views not on sheets
                var views = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => !v.IsTemplate && v.CanBePrinted)
                    .ToList();

                var placedViewIds = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .SelectMany(s => s.GetAllPlacedViews())
                    .Select(id => (long)id.Value)
                    .ToHashSet();

                var viewsNotOnSheet = views.Count(v => !placedViewIds.Contains((long)v.Id.Value));

                // CAD Imports
                var imports = new FilteredElementCollector(doc)
                    .OfClass(typeof(ImportInstance))
                    .Cast<ImportInstance>()
                    .ToList();

                var importInstances = imports.Count(i => i.IsLinked == false);
                var linkedImports = imports.Count(i => i.IsLinked);

                // In-place families
                var inPlaceFamilies = new FilteredElementCollector(doc)
                    .OfClass(typeof(Family))
                    .Cast<Family>()
                    .Count(f => f.IsInPlace);

                // Groups
                var groups = new FilteredElementCollector(doc)
                    .OfClass(typeof(Group))
                    .GetElementCount();

                // Design Options and Worksets
                var designOptions = 0;
                try
                {
                    designOptions = new FilteredElementCollector(doc)
                        .OfClass(typeof(DesignOption))
                        .GetElementCount();
                }
                catch { designOptions = 0; }

                var worksets = 0;
                try
                {
                    worksets = new FilteredWorksetCollector(doc).Cast<Workset>().Count();
                }
                catch { worksets = 0; }

                return new ModelHealthDto
                {
                    DocumentTitle = doc.Title,
                    Warnings = warnings,
                    RoomsTotal = roomsTotal,
                    RoomsUnplaced = roomsUnplaced,
                    ViewsNotOnSheet = viewsNotOnSheet,
                    ImportInstances = importInstances,
                    LinkedImports = linkedImports,
                    InPlaceFamilies = inPlaceFamilies,
                    Groups = groups,
                    DesignOptions = designOptions,
                    Worksets = worksets
                };
            }));
    }
}
