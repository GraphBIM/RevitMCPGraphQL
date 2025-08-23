using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ProjectLocationQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ProjectLocationType>("projectLocation")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new ProjectLocationDto();
                var site = doc.SiteLocation;
                var loc = new ProjectLocationDto
                {
                    SiteName = site?.PlaceName,
                    Latitude = site?.Latitude,
                    Longitude = site?.Longitude,
                    Elevation = site?.Elevation
                };
                return loc;
            }));
    }
}
