using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class PhasesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<PhaseType>>("phases")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<PhaseDto>();
                return doc.Phases.Cast<Phase>()
                    .Select(p => new PhaseDto { Id = p.Id?.Value ?? 0, Name = p.Name })
                    .OrderBy(p => p.Name)
                    .ToList();
            }));
    }
}
