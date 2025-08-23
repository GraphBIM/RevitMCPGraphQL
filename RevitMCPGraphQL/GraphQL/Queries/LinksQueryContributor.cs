using Autodesk.Revit.DB;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class LinksQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<LinkType>>("links")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<LinkDto>();

                var list = new FilteredElementCollector(doc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>()
                    .Select(i => new LinkDto
                    {
                        Id = i.Id?.Value ?? 0,
                        Name = i.Name,
                        Path = i.GetLinkDocument()?.PathName,
                        IsLoaded = i.GetLinkDocument() != null
                    })
                    .OrderBy(l => l.Name)
                    .ToList();

                return list;
            }));
    }
}
