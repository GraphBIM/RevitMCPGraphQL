using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class DeleteElementsContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<BooleanGraphType>("deleteElements")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<IdGraphType>>>> { Name = "elementIds" }
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return false;
                var ids = ctx.GetArgument<List<long>>("elementIds");
                return RevitDispatcher.Invoke(() =>
                {
                    using var t = new Transaction(doc, "Delete Elements");
                    t.Start();
                    try
                    {
                        var eidList = ids.Select(id => new ElementId(id)).ToList();
                        doc.Delete(eidList);
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
