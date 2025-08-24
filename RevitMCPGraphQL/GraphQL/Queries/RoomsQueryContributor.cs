using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;
using Autodesk.Revit.DB.Architecture;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class RoomsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<RoomType>>("rooms")
            .Arguments(new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "limit" },
                    new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(context =>
        {
            var limit = context.GetArgument<int?>("limit");
                   var documentId = context.GetArgument<long?>("documentId");
            return RevitDispatcher.Invoke(() =>
            {
                       var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<RoomDto>();
                var list = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<SpatialElement>()
                    .Select(r =>
                    {
                        // Build parameters
                        var parameters = r.Parameters
                            .Cast<Parameter>()
                            .Where(p => p != null && p.Definition != null)
                            .Select(p => new ParameterDto
                            {
                                Name = p.Definition!.Name,
                                Value = p.AsValueString(),
                            })
                            .ToList();

                        // BBox
                        var bbox = BoundingBoxBuilder.BuildBBoxDto(r, doc);

                        // Boundaries (outer/inner loops)
                        var loops = new List<List<double[]>>();
                        try
                        {
                            var opt = new SpatialElementBoundaryOptions
                            {
                                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish,
                                StoreFreeBoundaryFaces = false
                            };
                            var boundaries = r.GetBoundarySegments(opt);
                            foreach (var loop in boundaries)
                            {
                                var points = new List<double[]>();
                                foreach (var seg in loop)
                                {
                                    var crv = seg?.GetCurve();
                                    if (crv == null) continue;
                                    var sp = crv.GetEndPoint(0);
                                    var ep = crv.GetEndPoint(1);
                                    points.Add(new[] { sp.X, sp.Y, sp.Z });
                                    // Only add end point for last segment to close the loop
                                    // We'll ensure closure after collection
                                    points.Add(new[] { ep.X, ep.Y, ep.Z });
                                }
                                // Deduplicate consecutive duplicates
                                var compact = new List<double[]>();
                                double[]? prev = null;
                                foreach (var pt in points)
                                {
                                    if (prev == null || pt[0] != prev[0] || pt[1] != prev[1] || pt[2] != prev[2])
                                        compact.Add(pt);
                                    prev = pt;
                                }
                                loops.Add(compact);
                            }
                        }
                        catch { /* ignore boundary errors */ }

                        // Elements inside: use custom point-in-polygon test against boundary loops
                        var elementsInside = new List<long>();
                        try
                        {
                            if (loops.Count > 0)
                            {
                                // Optional prefilter by room's native bbox (internal units) for performance
                                ICollection<ElementId> candidates;
                                var nativeRoomBb = r.get_BoundingBox(null);
                                if (nativeRoomBb != null)
                                {
                                    var outline = new Outline(nativeRoomBb.Min, nativeRoomBb.Max);
                                    var bbfilter = new BoundingBoxIntersectsFilter(outline);
                                    candidates = new FilteredElementCollector(doc)
                                        .WherePasses(bbfilter)
                                        .WhereElementIsNotElementType()
                                        .ToElementIds();
                                }
                                else
                                {
                                    candidates = new FilteredElementCollector(doc)
                                        .WhereElementIsNotElementType()
                                        .ToElementIds();
                                }

                                foreach (var id in candidates)
                                {
                                    // Skip the room itself
                                    if (id.Value == r.Id.Value) continue;

                                    var el = doc.GetElement(id);
                                    if (el == null) continue;

                                    // Build candidate test points (XY plane)
                                    var testPoints = new List<XYZ>();
                                    var loc = el.Location;
                                    if (loc is LocationPoint lp)
                                    {
                                        testPoints.Add(lp.Point);
                                    }
                                    else if (loc is LocationCurve lc)
                                    {
                                        var c = lc.Curve;
                                        if (c != null)
                                        {
                                            testPoints.Add(c.GetEndPoint(0));
                                            testPoints.Add(c.Evaluate(0.5, true));
                                            testPoints.Add(c.GetEndPoint(1));
                                        }
                                    }
                                    else
                                    {
                                        var ebb = el.get_BoundingBox(null);
                                        if (ebb != null)
                                        {
                                            // center and XY-rectangle corners
                                            var center = (ebb.Min + ebb.Max) * 0.5;
                                            testPoints.Add(center);
                                            testPoints.Add(new XYZ(ebb.Min.X, ebb.Min.Y, center.Z));
                                            testPoints.Add(new XYZ(ebb.Max.X, ebb.Min.Y, center.Z));
                                            testPoints.Add(new XYZ(ebb.Max.X, ebb.Max.Y, center.Z));
                                            testPoints.Add(new XYZ(ebb.Min.X, ebb.Max.Y, center.Z));
                                        }
                                    }

                                    // Project to XY and test against loops
                                    var found = false;
                                    foreach (var p in testPoints)
                                    {
                                        if (PointInPolygon.IsPointInLoops2D(p.X, p.Y, loops))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (found)
                                        elementsInside.Add(id.Value);
                                }
                            }
                        }
                        catch { /* ignore */ }

                        return new RoomDto
                        {
                            Id = r.Id?.Value ?? 0,
                            Name = r.Name,
                            Number = r.Number,
                            Parameters = parameters,
                            BBox = bbox,
                            Boundaries = loops,
                            ElementsInside = elementsInside
                        };
                    })
                    .ToList();
                if (limit.HasValue) list = list.Take(limit.Value).ToList();
                return list;
            });
        });
    }
}
