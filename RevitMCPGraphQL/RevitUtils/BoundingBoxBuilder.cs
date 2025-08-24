using System;
using Autodesk.Revit.DB;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.RevitUtils;

internal static class BoundingBoxBuilder
{
    public static BoundingBoxDto? BuildBBoxDto(Element e, Document doc)
    {
        try
        {
            var bb = e.get_BoundingBox(null);
            if (bb == null || bb.Min == null || bb.Max == null) return null;

            var fo = doc.GetUnits().GetFormatOptions(SpecTypeId.Length);
            var ut = fo?.GetUnitTypeId();
            var acc = fo?.Accuracy;

            double Convert(double val)
            {
                try
                {
                    if (ut != null)
                    {
                        var converted = UnitUtils.ConvertFromInternalUnits(val, ut);
                        if (acc.HasValue && acc.Value > 0)
                        {
                            var decimals = AccuracyToDecimals(acc.Value);
                            return Math.Round(converted, decimals);
                        }
                        return converted;
                    }
                }
                catch { }
                return val; // fallback internal units
            }

            return new BoundingBoxDto
            {
                MinX = Convert(bb.Min.X),
                MinY = Convert(bb.Min.Y),
                MinZ = Convert(bb.Min.Z),
                MaxX = Convert(bb.Max.X),
                MaxY = Convert(bb.Max.Y),
                MaxZ = Convert(bb.Max.Z)
            };
        }
        catch
        {
            return null;
        }
    }

    private static int AccuracyToDecimals(double accuracy)
    {
        if (accuracy <= 0 || double.IsNaN(accuracy) || double.IsInfinity(accuracy))
            return 6;
        var decimals = (int)Math.Round(-Math.Log10(accuracy));
        return Math.Max(0, Math.Min(6, decimals));
    }
}
