using Autodesk.Revit.DB;
using System.Reflection;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class UnitsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ListGraphType<UnitGraphType>>("units")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<UnitDto>();

                var units = doc.GetUnits();

                var specProps = typeof(SpecTypeId)
                    .GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Where(p => p.PropertyType == typeof(ForgeTypeId));

                var results = new List<UnitDto>();
                foreach (var p in specProps)
                {
                    var spec = (ForgeTypeId)p.GetValue(null)!;
                    try
                    {
                        var fo = units.GetFormatOptions(spec);
                        if (fo == null) continue;
                        var ut = fo.GetUnitTypeId();
                        if (ut == null) continue;
                        var name = LabelUtils.GetLabelForUnit(ut);
                        string? symbolStr = null;
                        try
                        {
                            var sym = fo.GetSymbolTypeId();
                            if (sym != null)
                                symbolStr = LabelUtils.GetLabelForSymbol(sym);
                        }
                        catch { /* not all specs have symbol */ }

                        results.Add(new UnitDto
                        {
                            TypeId = ut.TypeId,
                            Name = name,
                            Symbol = symbolStr
                        });
                    }
                    catch
                    {
                        // ignore specs not supported by this document
                    }
                }

                return results
                    .GroupBy(u => u.TypeId)
                    .Select(g => g.First())
                    .OrderBy(u => u.Name)
                    .ToList();
            }));
    }
}
