using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;
using Autodesk.Revit.DB;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsByCategoryQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
    query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.ElementType>>("elementsByCategory")
            .Arguments(new QueryArguments(
                new QueryArgument<BuiltInCategoryEnum> { Name = "category" },
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
                var bic = ctx.GetArgument<Autodesk.Revit.DB.BuiltInCategory?>("category", null);
                var limit = ctx.GetArgument<int?>("limit");
                var collector = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType();
                if (bic.HasValue)
                    collector.OfCategory(bic.Value);
                var list = collector
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
