using GraphQL.Types;
using GraphQL;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class DesignOptionsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<DesignOptionType>>("designOptions")
            .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                var documentId = ctx.GetArgument<long?>("documentId");
                var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
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
