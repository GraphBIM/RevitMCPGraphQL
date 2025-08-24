using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;
using GraphQL;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class PhasesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<PhaseType>>("phases")
                .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<PhaseDto>();
                return doc.Phases.Cast<Phase>()
                    .Select(p => new PhaseDto { Id = p.Id?.Value ?? 0, Name = p.Name })
                    .OrderBy(p => p.Name)
                    .ToList();
            }));
    }
}
