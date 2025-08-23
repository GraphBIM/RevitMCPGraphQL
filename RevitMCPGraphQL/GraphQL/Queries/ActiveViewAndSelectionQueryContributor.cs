using Autodesk.Revit.DB;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ActiveViewAndSelectionQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Document?> getDoc)
    {
        query.Field<ViewGraphType>("activeView")
            .Resolve(_ => RevitDispatcher.Invoke(uiApp =>
            {
                var av = uiApp.ActiveUIDocument?.ActiveView;
                if (av == null) return null;
                return new ViewDto
                {
                    Id = av.Id?.Value ?? 0,
                    Name = av.Name,
                    ViewType = av.ViewType.ToString(),
                    IsTemplate = av.IsTemplate
                };
            }));

        query.Field<ListGraphType<IntGraphType>>("selection")
            .Resolve(_ => RevitDispatcher.Invoke(uiApp =>
            {
                var uidoc = uiApp.ActiveUIDocument;
                if (uidoc == null) return new List<int>();
                return uidoc.Selection.GetElementIds()
                    .Select(id => (int)id.Value)
                    .ToList();
            }));
    }
}
