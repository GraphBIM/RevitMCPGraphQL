using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using ElementType = RevitMCPGraphQL.GraphQL.Types.ElementType;
using RevitMCPGraphQL.RevitUtils;
using Autodesk.Revit.DB;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
    query.Field<ListGraphType<ElementType>>("elements")
            .Arguments(new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "categoryName" },
                new QueryArgument<StringGraphType> { Name = "familyName", Description = "Optional: filter elements by family name (case-insensitive)." },
                new QueryArgument<StringGraphType> { Name = "typeName", Description = "Optional: filter elements by type name (case-insensitive)." },
        new QueryArgument<IntGraphType> { Name = "limit" },
                   new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." },
                   new QueryArgument<BooleanGraphType> { Name = "isUnit", Description = "If true (default), parameter values include unit symbols; otherwise numeric only.", DefaultValue = true },
                   new QueryArgument<BooleanGraphType> { Name = "isIncludeTypeParams", Description = "If true, also include parameters from the element's type.", DefaultValue = false },
                   new QueryArgument<ListGraphType<StringGraphType>> { Name = "parameterNames", Description = "Optional: only include parameters whose names are in this list (case-insensitive)." }
            ))
            .Resolve(context =>
            {
                var categoryName = context.GetArgument<string>("categoryName");
                var familyNameArg = context.GetArgument<string>("familyName");
                var typeNameArg = context.GetArgument<string>("typeName");
                var limit = context.GetArgument<int?>("limit");
                   var documentId = context.GetArgument<long?>("documentId");
                var isUnit = context.GetArgument<bool>("isUnit", true);
                var includeTypeParams = context.GetArgument<bool>("isIncludeTypeParams", false);
                var parameterNames = context.GetArgument<List<string>>("parameterNames");
                var includeSet = (parameterNames != null && parameterNames.Count > 0)
                    ? new HashSet<string>(parameterNames, StringComparer.OrdinalIgnoreCase)
                    : null;
                return RevitDispatcher.Invoke(() =>
                {
                       var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                    if (doc == null) return new List<ElementDto>();
                    var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        var category = doc.Settings.Categories.Cast<Category>()
                            .FirstOrDefault(c => c.Name == categoryName);
                        if (category != null)
                        {
                            try
                            {
                                collector = collector.OfCategory((BuiltInCategory)category.Id.Value);
                            }
                            catch { }
                        }
                    }

                    // Materialize elements minimally and apply family/type filters first
                    var elems = collector.ToElements().Cast<Element>();

                    if (!string.IsNullOrWhiteSpace(familyNameArg) || !string.IsNullOrWhiteSpace(typeNameArg))
                    {
                        var famCmp = familyNameArg != null ? familyNameArg.Trim() : null;
                        var typeCmp = typeNameArg != null ? typeNameArg.Trim() : null;
                        elems = elems.Where(e =>
                        {
                            var tId = e.GetTypeId();
                            Autodesk.Revit.DB.ElementType? et = (tId != null && tId != ElementId.InvalidElementId)
                                ? doc.GetElement(tId) as Autodesk.Revit.DB.ElementType
                                : null;

                            // Type name match
                            if (!string.IsNullOrWhiteSpace(typeCmp))
                            {
                                var tn = et?.Name;
                                if (!string.Equals(tn, typeCmp, StringComparison.OrdinalIgnoreCase))
                                    return false;
                            }

                            // Family name via built-in parameter fallback to type
                            if (!string.IsNullOrWhiteSpace(famCmp))
                            {
                                string? fn = e.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM)?.AsString();
                                if (string.IsNullOrEmpty(fn) && et != null)
                                    fn = et.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM)?.AsString();
                                if (!string.Equals(fn, famCmp, StringComparison.OrdinalIgnoreCase))
                                    return false;
                            }

                            return true;
                        });
                    }

                    var list = elems
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
                });
            });
    }
}
