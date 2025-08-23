using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Executes.Inputs;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class CreateFamilyInstanceContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
    mutation.Field<IdGraphType>("createFamilyInstance")
            .Description("Creates a new family instance given a symbolId, location point, and optional levelId. Returns new elementId or null.")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "symbolId" },
                new QueryArgument<NonNullGraphType<PointInputType>> { Name = "location" },
                new QueryArgument<IdGraphType> { Name = "levelId" },
                new QueryArgument<StringGraphType> { Name = "structuralType" } // One of: NonStructural, Beam, Brace, Column, Footing
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return (object?)null;
                var symbolId = ctx.GetArgument<long>("symbolId");
                var loc = ctx.GetArgument<Dictionary<string, object>>("location");
                var levelId = ctx.GetArgument<long?>("levelId");
                var structuralTypeStr = ctx.GetArgument<string?>("structuralType");
                var x = Convert.ToDouble(loc["x"]);
                var y = Convert.ToDouble(loc["y"]);
                var z = Convert.ToDouble(loc["z"]);
                return RevitDispatcher.Invoke<object?>(() =>
                {
                    using var t = new Transaction(doc, "Create Family Instance");
                    t.Start();
                    try
                    {
                        var sym = doc.GetElement(new ElementId(symbolId)) as FamilySymbol;
                        if (sym == null) { t.RollBack(); return (object?)null; }
                        if (!sym.IsActive)
                            sym.Activate();
                        Level? level = null;
                        if (levelId.HasValue)
                            level = doc.GetElement(new ElementId(levelId.Value)) as Level;
                        var p = new XYZ(x, y, z);
                        var st = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;
                        if (!string.IsNullOrWhiteSpace(structuralTypeStr) && Enum.TryParse(structuralTypeStr, out Autodesk.Revit.DB.Structure.StructuralType parsed))
                            st = parsed;
                        var fi = doc.Create.NewFamilyInstance(p, sym, level, st);
                        var id = fi?.Id;
                        if (id == null) { t.RollBack(); return (object?)null; }
                        t.Commit();
                        return (object?)(long)id.Value;
                    }
                    catch
                    {
                        t.RollBack();
                        return (object?)null;
                    }
                });
            });
    }
}
