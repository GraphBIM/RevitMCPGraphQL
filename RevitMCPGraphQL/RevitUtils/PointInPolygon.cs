using System;
using System.Collections.Generic;

namespace RevitMCPGraphQL.RevitUtils;

internal static class PointInPolygon
{
    // Epsilon for boundary checks in feet (Revit internal units)
    private const double Eps = 1e-8;

    public static bool IsPointInLoops2D(double x, double y, List<List<double[]>> loops)
    {
        if (loops == null || loops.Count == 0) return false;
        // Even-odd rule across all loops; boundary => inside
        var inside = false;
        foreach (var loop in loops)
        {
            if (loop == null || loop.Count < 3) continue;

            // Convert to 2D; optionally drop duplicate last point if equals first
            var n = loop.Count;
            var first = loop[0];
            var last = loop[n - 1];
            if (ApproximatelyEqual(first[0], last[0]) && ApproximatelyEqual(first[1], last[1]))
            {
                n -= 1;
            }
            if (n < 3) continue;

            // Boundary check first
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var xi = loop[i][0]; var yi = loop[i][1];
                var xj = loop[j][0]; var yj = loop[j][1];
                if (IsPointOnSegment2D(x, y, xj, yj, xi, yi))
                    return true;
            }

            // Ray casting
            var odd = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var xi = loop[i][0]; var yi = loop[i][1];
                var xj = loop[j][0]; var yj = loop[j][1];

                var intersects = ((yi > y) != (yj > y)) &&
                                 (x < (xj - xi) * (y - yi) / ((yj - yi) + 0.0) + xi);
                if (intersects)
                    odd = !odd;
            }
            if (odd) inside = !inside;
        }
        return inside;
    }

    private static bool IsPointOnSegment2D(double x, double y, double x1, double y1, double x2, double y2)
    {
        // Bounding box check
        if (x < Math.Min(x1, x2) - Eps || x > Math.Max(x1, x2) + Eps ||
            y < Math.Min(y1, y2) - Eps || y > Math.Max(y1, y2) + Eps)
            return false;

        // Colinearity via cross product area ~ 0
        var cross = (x - x1) * (y2 - y1) - (y - y1) * (x2 - x1);
        return Math.Abs(cross) <= Eps;
    }

    private static bool ApproximatelyEqual(double a, double b)
        => Math.Abs(a - b) <= Eps;
}
