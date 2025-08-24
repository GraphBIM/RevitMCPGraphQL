using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public sealed class ElementType : ObjectGraphType<ElementDto>
{
    public ElementType()
    {
        Field(x => x.Id);
        Field(x => x.Name, nullable: true);
        Field(x => x.TypeId, nullable: true);
        Field<BoundingBoxType>("bbox")
            .Resolve(ctx => ctx.Source.BBox)
            .Description("Element bounding box (in document display units)");
        // Parameters as a key-value map: { "ParamName": "Value", ... }
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

public sealed class BoundingBoxType : ObjectGraphType<BoundingBoxDto>
{
    public BoundingBoxType()
    {
        Field(x => x.MinX);
        Field(x => x.MinY);
        Field(x => x.MinZ);
        Field(x => x.MaxX);
        Field(x => x.MaxY);
        Field(x => x.MaxZ);
    }
}