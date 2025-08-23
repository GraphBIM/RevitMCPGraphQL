using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class DesignOptionsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<DesignOptionType>>("designOptions")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<DesignOptionDto>();
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(DesignOption))
                    .Cast<DesignOption>()
                    .Select(o => new DesignOptionDto
                    {
                        Id = o.Id?.Value ?? 0,
                        Name = o.Name,
                        IsPrimary = o.IsPrimary
                    })
                    .OrderBy(o => o.Name)
                    .ToList();
            }));
    }
}
