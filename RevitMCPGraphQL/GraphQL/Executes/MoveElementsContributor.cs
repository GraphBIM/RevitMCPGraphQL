using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Executes.Inputs;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class MoveElementsContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<BooleanGraphType>("moveElements")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<IdGraphType>>>> { Name = "elementIds" },
                new QueryArgument<NonNullGraphType<VectorInputType>> { Name = "translation" }
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return false;
                var ids = ctx.GetArgument<List<long>>("elementIds");
                var v = ctx.GetArgument<Dictionary<string, object>>("translation");
                var dx = Convert.ToDouble(v["x"]);
                var dy = Convert.ToDouble(v["y"]);
                var dz = Convert.ToDouble(v["z"]);
                return RevitDispatcher.Invoke(() =>
                {
                    using var t = new Transaction(doc, "Move Elements");
                    t.Start();
                    try
                    {
                        var vec = new XYZ(dx, dy, dz);
                        var eidList = ids.Select(id => new ElementId(id)).ToList();
                        ElementTransformUtils.MoveElements(doc, eidList, vec);
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
