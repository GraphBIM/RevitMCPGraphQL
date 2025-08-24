using GraphQL.Types;
using GraphQL;
using RevitMCPGraphQL.GraphQL.Models;
using GridType = RevitMCPGraphQL.GraphQL.Types.GridType;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class GridsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<GridType>>("grids")
                .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                    var documentId = ctx.GetArgument<long?>("documentId");
                    var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<GridDto>();
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(Grid))
                    .Cast<Grid>()
                    .Select(g => new GridDto { Id = g.Id?.Value ?? 0, Name = g.Name })
                    .OrderBy(g => g.Name)
                    .ToList();
            }));
    }
}
