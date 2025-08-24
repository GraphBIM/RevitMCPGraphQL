using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class WorksetsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ListGraphType<WorksetType>>("worksets")
            .Description("Lists worksets from the active document or an optionally specified link document.")
            .Argument<IdGraphType>("documentId", "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document.")
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var hostDoc = getDoc();
                var requestedId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(hostDoc, requestedId);
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
