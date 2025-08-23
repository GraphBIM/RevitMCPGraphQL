using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ElementRelationshipType : ObjectGraphType<ElementRelationshipDto>
{
    public ElementRelationshipType()
    {
        Field(x => x.ElementId);
        Field(x => x.SuperComponentId, nullable: true);
        Field(x => x.HostId, nullable: true);
        Field<ListGraphType<IdGraphType>>("dependentIds").Resolve(ctx => ctx.Source.DependentIds);
        Field<ListGraphType<IdGraphType>>("joinedIds").Resolve(ctx => ctx.Source.JoinedIds);
    }
}
