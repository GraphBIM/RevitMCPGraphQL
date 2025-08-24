using System;
using System.Globalization;
using Autodesk.Revit.DB;

namespace RevitMCPGraphQL.RevitUtils;

internal static class ParameterValueFormatter
{
    public static string GetValue(Parameter p, Document doc, bool includeUnit)
    {
        if (p == null) return string.Empty;
        try
        {
            if (includeUnit)
            {
                // Prefer Revit's native formatting with units
                return p.AsValueString() ?? p.AsString() ?? string.Empty;
            }

            // Without unit symbol: try to return the numeric value in the document's display unit, but omit the unit text
            switch (p.StorageType)
            {
                case StorageType.String:
                    return p.AsString() ?? string.Empty;
                case StorageType.Integer:
                    return p.AsInteger().ToString(CultureInfo.InvariantCulture);
                case StorageType.Double:
                {
                    var internalVal = p.AsDouble();
                    try
                    {
                        var spec = p.Definition?.GetDataType();
                        if (spec != null)
                        {
                            var fo = doc.GetUnits().GetFormatOptions(spec);
                            var ut = fo?.GetUnitTypeId();
                            if (ut != null)
                            {
                                var converted = UnitUtils.ConvertFromInternalUnits(internalVal, ut);
                                // Round according to the format accuracy when available
                                try
                                {
                                    var acc = fo?.Accuracy;
                                    if (acc.HasValue && acc.Value > 0)
                                    {
                                        var decimals = AccuracyToDecimals(acc.Value);
                                        return Math.Round(converted, decimals).ToString(CultureInfo.InvariantCulture);
                                    }
                                }
                                catch { /* fall back to raw converted string */ }

                                return converted.ToString(CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    catch { /* fall back to internal units */ }

                    // Fallback: internal units numeric
                    return internalVal.ToString(CultureInfo.InvariantCulture);
                }
                case StorageType.ElementId:
                    return (p.AsElementId()?.IntegerValue ?? ElementId.InvalidElementId.IntegerValue)
                        .ToString(CultureInfo.InvariantCulture);
                default:
                    return p.AsString() ?? string.Empty;
            }
        }
        catch
        {
            // Safe fallback depending on requested format
            return includeUnit ? (p.AsValueString() ?? p.AsString() ?? string.Empty) : (p.AsString() ?? string.Empty);
        }
    }

    private static int AccuracyToDecimals(double accuracy)
    {
        // Convert an accuracy step (e.g., 0.001) to a reasonable number of decimals
        if (accuracy <= 0 || double.IsNaN(accuracy) || double.IsInfinity(accuracy))
            return 6;
        var decimals = (int)Math.Round(-Math.Log10(accuracy));
        return Math.Max(0, Math.Min(6, decimals));
    }
}
