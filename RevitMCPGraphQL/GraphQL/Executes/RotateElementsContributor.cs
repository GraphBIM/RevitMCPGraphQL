using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Executes.Inputs;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class RotateElementsContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<BooleanGraphType>("rotateElements")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<IdGraphType>>>> { Name = "elementIds" },
                new QueryArgument<NonNullGraphType<PointInputType>> { Name = "point" },
                new QueryArgument<NonNullGraphType<VectorInputType>> { Name = "axis" },
                new QueryArgument<NonNullGraphType<FloatGraphType>> { Name = "angle" }
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return false;
                var ids = ctx.GetArgument<List<long>>("elementIds");
                var p = ctx.GetArgument<Dictionary<string, object>>("point");
                var a = ctx.GetArgument<Dictionary<string, object>>("axis");
                var angle = ctx.GetArgument<double>("angle");
                var px = Convert.ToDouble(p["x"]);
                var py = Convert.ToDouble(p["y"]);
                var pz = Convert.ToDouble(p["z"]);
                var ax = Convert.ToDouble(a["x"]);
                var ay = Convert.ToDouble(a["y"]);
                var az = Convert.ToDouble(a["z"]);
                return RevitDispatcher.Invoke(() =>
                {
                    using var t = new Transaction(doc, "Rotate Elements");
                    t.Start();
                    try
                    {
                        var line = Line.CreateUnbound(new XYZ(px, py, pz), new XYZ(ax, ay, az));
                        var eidList = ids.Select(id => new ElementId(id)).ToList();
                        ElementTransformUtils.RotateElements(doc, eidList, line, angle);
                        t.Commit();
                        return true;
                    }
                    catch
                    {
                        t.RollBack();
                        return false;
                    }
                });
            });
    }
}
