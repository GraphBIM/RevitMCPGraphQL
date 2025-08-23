using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ParameterType : ObjectGraphType<ParameterDto>
{
    public ParameterType()
    {
        Field(x => x.Name, nullable: true)
            .Description("Name of the parameter");
        Field(x => x.Value, nullable: true)
            .Description("Value of the parameter");
    }
}