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
    }
}