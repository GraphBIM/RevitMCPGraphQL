using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using DocumentType = RevitMCPGraphQL.GraphQL.Types.DocumentType;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class DocumentQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<DocumentType>("document").Resolve(_ =>
        {
            return RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
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
