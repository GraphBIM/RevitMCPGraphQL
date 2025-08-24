using GraphQL.Types;
using GraphQL;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class CoordinatesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        // Returns key coordinate points for the project
        query.Field<CoordinatesType>("coordinates")
            .Arguments(new QueryArguments(
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return null;

                // Collect base points
                var points = new List<BasePointDto>();

                try
                {
                    var collector = new FilteredElementCollector(doc)
                        .OfClass(typeof(BasePoint))
                        .Cast<BasePoint>();

                    foreach (var bp in collector)
                    {
                        var p = bp.Position; // XYZ in internal units
                        points.Add(new BasePointDto
                        {
                            Id = bp.Id?.Value ?? 0,
                            Name = bp.Name,
                            IsShared = bp.IsShared,
                            X = ToDisplayLength(doc, p?.X ?? 0),
                            Y = ToDisplayLength(doc, p?.Y ?? 0),
                            Z = ToDisplayLength(doc, p?.Z ?? 0)
                        });
                    }
                }
                catch { }

                return new CoordinatesDto
                {
                    ProjectBasePoint = points.FirstOrDefault(x => !x.IsShared),
                    SharedBasePoint = points.FirstOrDefault(x => x.IsShared),
                    AllBasePoints = points
                };
            }));

        // Individual lists for convenience
        query.Field<BasePointType>("projectBasePoint")
            .Arguments(new QueryArguments(
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return null;
                return GetBasePoints(doc).FirstOrDefault(x => !x.IsShared);
            }));

        query.Field<BasePointType>("sharedBasePoint")
            .Arguments(new QueryArguments(
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return null;
                return GetBasePoints(doc).FirstOrDefault(x => x.IsShared);
            }));

        query.Field<ListGraphType<BasePointType>>("basePoints")
            .Arguments(new QueryArguments(
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<BasePointDto>();
                return GetBasePoints(doc);
            }));
    }

    private static List<BasePointDto> GetBasePoints(Document doc)
    {
        var points = new List<BasePointDto>();
        try
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>();

            foreach (var bp in collector)
            {
                var p = bp.Position;
                points.Add(new BasePointDto
                {
                    Id = bp.Id?.Value ?? 0,
                    Name = bp.Name,
                    IsShared = bp.IsShared,
                    X = ToDisplayLength(doc, p?.X ?? 0),
                    Y = ToDisplayLength(doc, p?.Y ?? 0),
                    Z = ToDisplayLength(doc, p?.Z ?? 0)
                });
            }
        }
        catch { }
        return points;
    }

    private static double ToDisplayLength(Document doc, double internalFeet)
    {
        try
        {
            var fo = doc.GetUnits().GetFormatOptions(SpecTypeId.Length);
            var ut = fo?.GetUnitTypeId();
            var acc = fo?.Accuracy;
            if (ut != null)
            {
                var converted = UnitUtils.ConvertFromInternalUnits(internalFeet, ut);
                if (acc.HasValue && acc.Value > 0)
                {
                    var decimals = AccuracyToDecimals(acc.Value);
                    return Math.Round(converted, decimals);
                }
                return converted;
            }
        }
        catch { }
        return internalFeet;
    }

    private static int AccuracyToDecimals(double accuracy)
    {
        if (accuracy <= 0 || double.IsNaN(accuracy) || double.IsInfinity(accuracy))
            return 6;
        var decimals = (int)Math.Round(-Math.Log10(accuracy));
        return Math.Max(0, Math.Min(6, decimals));
    }
}
