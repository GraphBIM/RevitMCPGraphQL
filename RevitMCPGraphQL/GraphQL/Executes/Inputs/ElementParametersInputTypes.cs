using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Executes.Inputs;

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
