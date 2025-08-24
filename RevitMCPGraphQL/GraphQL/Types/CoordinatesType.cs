using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class CoordinatesType : ObjectGraphType<CoordinatesDto>
{
    public CoordinatesType()
    {
        Field<BasePointType>("projectBasePoint");
        Field<BasePointType>("sharedBasePoint");
        Field<ListGraphType<BasePointType>>("allBasePoints");
    }
}
