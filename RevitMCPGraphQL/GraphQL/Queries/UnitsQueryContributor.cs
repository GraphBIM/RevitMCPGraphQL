using System.Reflection;
using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class UnitsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ListGraphType<UnitGraphType>>("units")
            .Description("Lists unit settings for the active document or an optionally specified link document.")
            .Argument<IdGraphType>("documentId", "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document.")
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var hostDoc = getDoc();
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(hostDoc, documentId);
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
