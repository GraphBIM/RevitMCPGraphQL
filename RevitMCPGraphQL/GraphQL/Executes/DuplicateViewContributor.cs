using GraphQL;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class DuplicateViewContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<IdGraphType>("duplicateView")
            .Description("Duplicate a view; returns the new view Id or null on failure.")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "viewId" },
                new QueryArgument<BooleanGraphType> { Name = "withDetailing" }
            ))
            .Resolve(ctx =>
            {
                var doc = getDoc();
                if (doc == null) return (object?)null;
                var viewIdLong = ctx.GetArgument<long>("viewId");
                var withDetailing = ctx.GetArgument<bool>("withDetailing");
                return RevitDispatcher.Invoke<object?>(() =>
                {
                    using var t = new Transaction(doc, "Duplicate View");
                    t.Start();
                    try
                    {
                        var v = doc.GetElement(new ElementId(viewIdLong)) as View;
                        if (v == null) { t.RollBack(); return (object?)null; }
                        var option = withDetailing ? ViewDuplicateOption.WithDetailing : ViewDuplicateOption.Duplicate;
                        var newId = v.Duplicate(option);
                        t.Commit();
                        return (object?)(long)newId.Value;
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
