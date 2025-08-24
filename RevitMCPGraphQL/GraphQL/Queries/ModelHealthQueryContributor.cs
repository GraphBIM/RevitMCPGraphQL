using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ModelHealthQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ModelHealthType>("modelHealth")
            .Argument<IdGraphType>("documentId",
                "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document.")
            .Description("Quick model health summary for the active document.")
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var hostDoc = getDoc();
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(hostDoc, documentId);
                if (doc == null) return new ModelHealthDto();

                // Warnings
                var warnings = 0;
                try
                {
                    warnings = doc.GetWarnings()?.Count ?? 0;
                }
                catch
                {
                    warnings = 0;
                }

                // Element counts
                var elementsTotal = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .GetElementCount();

                // Rooms
                var rooms = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements();

                var roomsTotal = rooms.Count;
                var roomsUnplaced = rooms
                    .OfType<Room>()
                    .Count(r => r.Location == null);
                var roomsPlaced = roomsTotal - roomsUnplaced;

                // Areas
                var areas = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Areas)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .OfType<Area>()
                    .ToList();
                var areasTotal = areas.Count;
                var areasUnplaced = areas.Count(a => a.Location == null);

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
                var viewsOnSheet = views.Count(v => placedViewIds.Contains((long)v.Id.Value));
                var viewsNotOnSheet = views.Count - viewsOnSheet;
                var viewsTotal = views.Count;

                // View templates used
                var viewTemplatesUsed = views.Count(v => v.ViewTemplateId != ElementId.InvalidElementId);

                // CAD Imports
                var imports = new FilteredElementCollector(doc)
                    .OfClass(typeof(ImportInstance))
                    .Cast<ImportInstance>()
                    .ToList();

                var importInstances = imports.Count(i => i.IsLinked == false);
                var linkedImports = imports.Count(i => i.IsLinked);

                // Revit Links status
                var revitLinks = new FilteredElementCollector(doc)
                    .OfClass(typeof(RevitLinkType))
                    .Cast<RevitLinkType>()
                    .ToList();
                var revitLinksTotal = revitLinks.Count;
                var revitLinksLoaded = revitLinks.Count(t => RevitLinkType.IsLoaded(doc, t.Id));
                var revitLinksUnloaded = revitLinksTotal - revitLinksLoaded;

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
                catch
                {
                    designOptions = 0;
                }

                var designOptionSets = 0;
                try
                {
                    var setIds = new HashSet<int>();
                    foreach (var opt in new FilteredElementCollector(doc).OfClass(typeof(DesignOption))
                                 .Cast<DesignOption>())
                    {
                        var p = opt.get_Parameter(BuiltInParameter.OPTION_SET_ID);
                        if (p != null && p.HasValue)
                            setIds.Add(p.AsInteger());
                    }

                    designOptionSets = setIds.Count;
                }
                catch
                {
                    designOptionSets = 0;
                }

                var worksets = 0;
                try
                {
                    worksets = new FilteredWorksetCollector(doc).Cast<Workset>().Count();
                }
                catch
                {
                    worksets = 0;
                }

                var workshared = false;
                try
                {
                    workshared = doc.IsWorkshared;
                }
                catch
                {
                    workshared = false;
                }

                return new ModelHealthDto
                {
                    DocumentTitle = doc.Title,
                    Warnings = warnings,
                    ElementsTotal = elementsTotal,
                    RoomsTotal = roomsTotal,
                    RoomsUnplaced = roomsUnplaced,
                    RoomsPlaced = roomsPlaced,
                    AreasTotal = areasTotal,
                    AreasUnplaced = areasUnplaced,
                    Levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).GetElementCount(),
                    Phases = new FilteredElementCollector(doc).OfClass(typeof(Phase)).GetElementCount(),
                    ViewsNotOnSheet = viewsNotOnSheet,
                    ViewsOnSheet = viewsOnSheet,
                    ViewsTotal = viewsTotal,
                    ViewTemplatesUsed = viewTemplatesUsed,
                    ImportInstances = importInstances,
                    LinkedImports = linkedImports,
                    RevitLinksTotal = revitLinksTotal,
                    RevitLinksLoaded = revitLinksLoaded,
                    RevitLinksUnloaded = revitLinksUnloaded,
                    InPlaceFamilies = inPlaceFamilies,
                    Groups = groups,
                    DesignOptions = designOptions,
                    DesignOptionSets = designOptionSets,
                    Worksets = worksets,
                    Workshared = workshared
                };
            }));
    }
}