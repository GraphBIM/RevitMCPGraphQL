using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.RevitUtils;

internal static class CombinedParametersBuilder
{
    public static List<ParameterDto> BuildCombinedParameters(Element e, Document doc, bool isUnit, bool includeTypeParams, ISet<string>? includeSet)
    {
        var list = new List<ParameterDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in e.Parameters.Cast<Parameter>())
        {
            if (p?.Definition == null) continue;
            var name = p.Definition!.Name;
            if (includeSet != null && !includeSet.Contains(name)) continue;
            if (seen.Add(name))
                list.Add(new ParameterDto { Name = name, Value = ParameterValueFormatter.GetValue(p, doc, isUnit) });
        }

        if (includeTypeParams)
        {
            try
            {
                var typeId = e.GetTypeId();
                if (typeId != null)
                {
                    var typeElem = doc.GetElement(typeId);
                    if (typeElem != null)
                    {
                        foreach (var p in typeElem.Parameters.Cast<Parameter>())
                        {
                            if (p?.Definition == null) continue;
                            var name = p.Definition!.Name;
                            if (includeSet != null && !includeSet.Contains(name)) continue;
                            if (seen.Add(name))
                                list.Add(new ParameterDto { Name = name, Value = ParameterValueFormatter.GetValue(p, doc, isUnit) });
                        }
                    }
                }
            }
            catch { /* ignore type param errors */ }
        }

        return list;
    }
}
