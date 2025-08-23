using GraphQL;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class SetElementParameterContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<BooleanGraphType>("setElementParameter")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "elementId" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "parameterName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return false;
                var elementId = ctx.GetArgument<long>("elementId");
                var name = ctx.GetArgument<string>("parameterName");
                var value = ctx.GetArgument<string>("value");
                return RevitDispatcher.Invoke(() =>
                {
                    using var t = new Transaction(doc, "Set Parameter");
                    t.Start();
                    var el = doc.GetElement(new ElementId(elementId));
                    if (el == null) { t.RollBack(); return false; }
                    var p = el.LookupParameter(name);
                    var ok = p != null && !p.IsReadOnly && p.Set(value);
                    if (ok) t.Commit(); else t.RollBack();
                    return ok;
                });
            });
    }
}
