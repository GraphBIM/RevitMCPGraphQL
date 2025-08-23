using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class WorksetsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<WorksetType>>("worksets")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<WorksetDto>();
                var ws = new FilteredWorksetCollector(doc)
                    .Cast<Workset>()
                    .Select(w => new WorksetDto
                    {
                        Id = (long)w.Id.IntegerValue,
                        Name = w.Name,
                        Kind = w.Kind.ToString(),
                        Owner = null,
                        IsOpen = null
                    })
                    .OrderBy(w => w.Name)
                    .ToList();
                return ws;
            }));
    }
}
