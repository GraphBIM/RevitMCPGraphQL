using Autodesk.Revit.DB;
using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using DocumentType = RevitMCPGraphQL.GraphQL.Types.DocumentType;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class DocumentQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<DocumentType>("document")
        .Description("Returns basic information about the active document or an optionally specified link document.")
        .Argument<IdGraphType>("documentId", "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document.")
            .Resolve(ctx =>
            {
                return RevitDispatcher.Invoke(() =>
                {
                    var hostDoc = getDoc();
            var requestedId = ctx.GetArgument<long?>("documentId");
            var doc = DocumentResolver.ResolveDocument(hostDoc, requestedId);
                    if (doc == null)
                        return new DocumentDto { Title = string.Empty, PathName = null, IsFamilyDocument = false };

                    return new DocumentDto
                    {
                        Title = doc.Title ?? string.Empty,
                        PathName = doc.PathName,
                        IsFamilyDocument = doc.IsFamilyDocument,
                    };
                });
            });
    }
}
