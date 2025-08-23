using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ScheduleType : ObjectGraphType<ScheduleDto>
{
    public ScheduleType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.IsTemplate, nullable: true);
    }
}
