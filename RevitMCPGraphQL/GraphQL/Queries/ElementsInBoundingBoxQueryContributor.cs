using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.RevitUtils;
using Autodesk.Revit.DB;

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
                new QueryArgument<IntGraphType> { Name = "limit" },
                       new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." },
                       new QueryArgument<BooleanGraphType> { Name = "isUnit", Description = "If true (default), parameter values include unit symbols; otherwise numeric only.", DefaultValue = true },
                       new QueryArgument<BooleanGraphType> { Name = "isIncludeTypeParams", Description = "If true, also include parameters from the element's type.", DefaultValue = false },
                       new QueryArgument<ListGraphType<StringGraphType>> { Name = "parameterNames", Description = "Optional: only include parameters whose names are in this list (case-insensitive)." }
            ))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                       var documentId = ctx.GetArgument<long?>("documentId");
                       var isUnit = ctx.GetArgument<bool>("isUnit", true);
                       var includeTypeParams = ctx.GetArgument<bool>("isIncludeTypeParams", false);
                       var parameterNames = ctx.GetArgument<List<string>>("parameterNames");
                       var includeSet = (parameterNames != null && parameterNames.Count > 0)
                           ? new HashSet<string>(parameterNames, StringComparer.OrdinalIgnoreCase)
                           : null;
                       var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
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
                    .Cast<Element>()
                    .Select(e => new ElementDto
                    {
                        Id = e.Id?.Value ?? 0,
                        TypeId = e.GetTypeId()?.Value,
                        Name = e.Name,
                        Parameters = CombinedParametersBuilder.BuildCombinedParameters(e, doc, isUnit, includeTypeParams, includeSet),
                        BBox = BoundingBoxBuilder.BuildBBoxDto(e, doc)
                    })
                    .ToList();
                if (limit.HasValue) list = list.Take(limit.Value).ToList();
                return list;
            }));
    }
}
