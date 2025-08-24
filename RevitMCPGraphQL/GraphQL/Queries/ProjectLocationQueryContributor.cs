using GraphQL.Types;
using GraphQL;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ProjectLocationQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ProjectLocationType>("projectLocation")
            .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var docId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(getDoc(), docId);
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
