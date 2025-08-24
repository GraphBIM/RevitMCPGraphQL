using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;
using GraphQL;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementTypesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<ElementTypeGraphType>>("elementTypes")
                .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<ElementTypeDto>();
                return new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .ToElements()
                    // .Select(e => (ElementType)e)
                    .Select(t => new ElementTypeDto
                    {
                        Id = t.Id?.Value ?? 0,
                        Name = t.Name,
                        CategoryName = t.Category?.Name,
                        Parameters = t.Parameters
                            .Cast<Parameter>()
                            .Where(p => p?.Definition != null)
                            .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = p.AsValueString() })
                            .ToList()
                    })
                    .OrderBy(t => t.CategoryName)
                    .ThenBy(t => t.Name)
                    .ToList();
            }));
    }
}
