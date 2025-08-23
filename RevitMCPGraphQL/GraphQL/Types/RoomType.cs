using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class RoomType : ObjectGraphType<RoomDto>
{
    public RoomType()
    {
        Field(x => x.Id).Description("Room ID");
        Field(x => x.Name, nullable: true).Description("Room name");
        Field(x => x.Number, nullable: true).Description("Room number");
        Field(x => x.Area).Description("Room area");
    }
}