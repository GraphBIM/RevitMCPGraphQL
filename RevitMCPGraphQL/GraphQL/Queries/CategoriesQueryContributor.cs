using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using CategoryType = RevitMCPGraphQL.GraphQL.Types.CategoryType;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class CategoriesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ListGraphType<CategoryType>>("categories")
            .Arguments(new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "limit" },
                new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(context =>
            {
                return RevitDispatcher.Invoke(() =>
                {
                    var hostDoc = getDoc();
                    var requestedId = context.GetArgument<long?>("documentId");
                    var doc = DocumentResolver.ResolveDocument(hostDoc, requestedId);
                    if (doc == null) return new List<CategoryDto>();
                    var categories = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .Select(e => e.Category)
                        .Where(c => c != null)
                        .GroupBy(c => c.Id)
                        .Select(g => g.First())
                        .Where(c => c != null)
                        .Select(c =>
                        {
                            string? bicName = null;
                            try
                            {
                                var intId = c.Id.Value;
                                if (System.Enum.IsDefined(typeof(BuiltInCategory), intId))
                                    bicName = ((BuiltInCategory)intId).ToString();
                            }
                            catch { }

                            string? typeName = null;
                            try { typeName = c.CategoryType.ToString(); }
                            catch { }

                            bool isCuttable = false;
                            try { isCuttable = c.IsCuttable; }
                            catch { }

                            bool allowsBound = false;
                            try { allowsBound = c.AllowsBoundParameters; }
                            catch { }

                            bool isTag = false;
                            try { isTag = c.IsTagCategory; }
                            catch { }

                            bool canAddSub = false;
                            try { canAddSub = c.CanAddSubcategory; }
                            catch { }

                            return new CategoryDto
                            {
                                Id = c.Id!.Value,
                                Name = c.Name,
                                BuiltInCategory = bicName,
                                CategoryType = typeName,
                                IsCuttable = isCuttable,
                                AllowsBoundParameters = allowsBound,
                                IsTagCategory = isTag,
                                CanAddSubcategory = canAddSub
                            };
                        })
                        .OrderBy(c => c.Name)
                        .ToList();
                    var limit = context.GetArgument<int?>("limit");
                    if (limit.HasValue)
                        categories = categories.Take(limit.Value).ToList();

                    return categories;
                });
            });
    }
}
