using GraphQL;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL
{

    public class ParameterSetInputType : InputObjectGraphType
    {
        public ParameterSetInputType()
        {
            Name = "ParameterSetInput";
            Field<NonNullGraphType<StringGraphType>>("parameterName");
            Field<NonNullGraphType<StringGraphType>>("value");
        }
    }

    public class ElementParametersInputType : InputObjectGraphType
    {
        public ElementParametersInputType()
        {
            Name = "ElementParametersInput";
            Field<NonNullGraphType<IdGraphType>>("elementId");
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<ParameterSetInputType>>>>("parameters");
        }
    }

    public class RevitExecuteProvider : ObjectGraphType
    {
        public RevitExecuteProvider(Func<Document?> getDoc)
        {
            Field<BooleanGraphType>("setElementParametersBatch")
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
}
