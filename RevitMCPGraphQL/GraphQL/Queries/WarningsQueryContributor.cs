using Autodesk.Revit.DB;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class WarningsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<WarningType>>("warnings")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<WarningDto>();
        return doc.GetWarnings()
                    .Select(w => new WarningDto
                    {
                        Description = w.GetDescriptionText(),
            ElementIds = w.GetFailingElements().Select(id => id.Value).ToList()
                    })
                    .ToList();
            }));
    }
}
