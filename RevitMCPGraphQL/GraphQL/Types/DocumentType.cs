using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;

namespace RevitMCPGraphQL.GraphQL.Types;

public class DocumentType : ObjectGraphType<DocumentDto>
{
    public DocumentType()
    {
        Field(x => x.Title).Description("Document title");
        Field(x => x.PathName, nullable: true).Description("Full document path, if saved");
        Field(x => x.IsFamilyDocument).Description("Whether the document is a family");
    }
}