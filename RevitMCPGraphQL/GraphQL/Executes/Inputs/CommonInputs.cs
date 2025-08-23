using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Executes.Inputs;

public class PointInputType : InputObjectGraphType
{
    public PointInputType()
    {
        Name = "PointInput";
        Field<NonNullGraphType<FloatGraphType>>("x");
        Field<NonNullGraphType<FloatGraphType>>("y");
        Field<NonNullGraphType<FloatGraphType>>("z");
    }
}

public class VectorInputType : InputObjectGraphType
{
    public VectorInputType()
    {
        Name = "VectorInput";
        Field<NonNullGraphType<FloatGraphType>>("x");
        Field<NonNullGraphType<FloatGraphType>>("y");
        Field<NonNullGraphType<FloatGraphType>>("z");
    }
}
