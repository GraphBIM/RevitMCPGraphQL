namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class DocumentDto
{
    public string Title { get; set; } = string.Empty;
    public string? PathName { get; set; }
    public bool IsFamilyDocument { get; set; }
}