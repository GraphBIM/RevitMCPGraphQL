using GraphQL;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class SetElementTypeContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<BooleanGraphType>("setElementType")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "elementId" },
                new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "typeId" }
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return false;
                var elementId = ctx.GetArgument<long>("elementId");
                var typeId = ctx.GetArgument<long>("typeId");
                return RevitDispatcher.Invoke(() =>
                {
                    using var t = new Transaction(doc, "Set Element Type");
                    t.Start();
                    var el = doc.GetElement(new ElementId(elementId));
                    var newTypeId = new ElementId(typeId);
                    if (el == null) { t.RollBack(); return false; }
                    try
                    {
                        el.ChangeTypeId(newTypeId);
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
