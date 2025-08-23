using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using GridType = RevitMCPGraphQL.GraphQL.Types.GridType;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class GridsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<GridType>>("grids")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
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
