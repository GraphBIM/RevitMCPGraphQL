using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsInBoundingBoxQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
    query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.ElementType>>("elementsInBoundingBox")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<FloatGraphType>> { Name = "minX" },
                new QueryArgument<NonNullGraphType<FloatGraphType>> { Name = "minY" },
                new QueryArgument<NonNullGraphType<FloatGraphType>> { Name = "minZ" },
                new QueryArgument<NonNullGraphType<FloatGraphType>> { Name = "maxX" },
                new QueryArgument<NonNullGraphType<FloatGraphType>> { Name = "maxY" },
                new QueryArgument<NonNullGraphType<FloatGraphType>> { Name = "maxZ" },
                new QueryArgument<IntGraphType> { Name = "limit" }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<ElementDto>();

                var minX = ctx.GetArgument<double>("minX");
                var minY = ctx.GetArgument<double>("minY");
                var minZ = ctx.GetArgument<double>("minZ");
                var maxX = ctx.GetArgument<double>("maxX");
                var maxY = ctx.GetArgument<double>("maxY");
                var maxZ = ctx.GetArgument<double>("maxZ");
                var limit = ctx.GetArgument<int?>("limit");

                var outline = new Outline(new XYZ(minX, minY, minZ), new XYZ(maxX, maxY, maxZ));
                var bbFilter = new BoundingBoxIntersectsFilter(outline, true);

                var list = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .WherePasses(bbFilter)
                    .ToElements()
                    .Select(e => new ElementDto
                    {
                        Id = e.Id?.Value ?? 0,
                        Name = e.Name,
                        Parameters = e.Parameters
                            .Cast<Parameter>()
                            .Where(p => p != null && p.Definition != null)
                            .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = p.AsValueString() })
                            .ToList()
                    })
                    .ToList();
                if (limit.HasValue) list = list.Take(limit.Value).ToList();
                return list;
            }));
    }
}
