using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class RoomType : ObjectGraphType<RoomDto>
{
    public RoomType()
    {
        Field(x => x.Id).Description("Room ID");
        Field(x => x.Name, nullable: true).Description("Room name");
        Field(x => x.Number, nullable: true).Description("Room number");
         Field<StringMapScalarType>("parameters")
            .Resolve(context =>
            {
                var list = context.Source.Parameters ?? new List<ParameterDto>();
                // In case of duplicate names, keep the first occurrence
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in list)
                {
                    if (p == null || string.IsNullOrEmpty(p.Name)) continue;
                    var key = p.Name;
                    if (!dict.ContainsKey(key))
                        dict[key] = p.Value ?? string.Empty;
                }

                return dict;
            })
            .Description("Parameters as a key-value map, keyed by parameter name");

        Field<BoundingBoxType>("bbox")
            .Description("Axis-aligned bounding box of the room in display units.")
            .Resolve(ctx => ctx.Source.BBox);

        Field<ListGraphType<ListGraphType<ListGraphType<FloatGraphType>>>>("boundaries")
            .Description("Room boundary loops; each point is [x,y,z] in display units.")
            .Resolve(ctx => ctx.Source.Boundaries);

        Field<ListGraphType<IdGraphType>>("elementsInside")
            .Description("IDs of elements whose bounding boxes intersect the room.")
            .Resolve(ctx => ctx.Source.ElementsInside);
    }
}