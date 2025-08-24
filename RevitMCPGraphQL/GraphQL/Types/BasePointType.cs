using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class BasePointType : ObjectGraphType<BasePointDto>
{
    public BasePointType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.IsShared);
        Field(x => x.X);
        Field(x => x.Y);
        Field(x => x.Z);
    }
}
