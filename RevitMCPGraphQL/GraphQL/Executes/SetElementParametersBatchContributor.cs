using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Executes.Inputs;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal sealed class SetElementParametersBatchContributor : IMutationContributor
{
    public void Register(ObjectGraphType mutation, Func<Document?> getDoc)
    {
        mutation.Field<BooleanGraphType>("setElementParametersBatch")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<ListGraphType<NonNullGraphType<ElementParametersInputType>>>> { Name = "inputs" }
            ))
            .Resolve(context =>
            {
                var doc = getDoc();
                if (doc == null) return false;
                var inputs = context.GetArgument<List<Dictionary<string, object>>>("inputs");
                return RevitDispatcher.Invoke(() =>
                {
                    bool anyChange = false;
                    using (var trans = new Transaction(doc, "Set Multiple Parameters"))
                    {
                        trans.Start();
                        foreach (var input in inputs)
                        {
                            var elementId = (long)input["elementId"];
                            var paramList = input["parameters"] as IEnumerable<object>;
                            var element = doc.GetElement(new ElementId(elementId));
                            if (element == null || paramList == null) continue;
                            foreach (var paramObj in paramList)
                            {
                                var paramDict = paramObj as Dictionary<string, object>;
                                var paramName = (string)paramDict!["parameterName"];
                                var value = (string)paramDict["value"];
                                var param = element.LookupParameter(paramName);
                                if (param != null && !param.IsReadOnly && param.Set(value))
                                    anyChange = true;
                            }
                        }
                        if (anyChange)
                            trans.Commit();
                        else
                            trans.RollBack();
                        return anyChange;
                    }
                });
            });
    }
}
