using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class WarningsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ListGraphType<WarningType>>("warnings")
            .Description("Lists reviewable warnings from the active document or an optionally specified link document.")
            .Argument<IdGraphType>("documentId", "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document.")
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var hostDoc = getDoc();
                var requestedId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(hostDoc, requestedId);
                if (doc == null) return new List<WarningDto>();
                var warnings = doc.GetWarnings() ?? new List<FailureMessage>();
                return warnings
                    .Select(w => new WarningDto
                    {
                        Description = w.GetDescriptionText(),
                        ElementIds = w.GetFailingElements().Select(id => id.Value).ToList()
                    })
                    .ToList();
            }));
    }
}
