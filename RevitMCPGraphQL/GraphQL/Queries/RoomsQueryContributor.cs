using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class RoomsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<RoomType>>("rooms").Resolve(context =>
        {
            return RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<RoomDto>();
                return new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<SpatialElement>()
                    .Select(r => new RoomDto
                    {
                        Id = r.Id?.Value ?? 0,
                        Name = r.Name,
                        Number = r.Number,
                        Area = r.Area
                    })
                    .ToList();
            });
        });
    }
}
