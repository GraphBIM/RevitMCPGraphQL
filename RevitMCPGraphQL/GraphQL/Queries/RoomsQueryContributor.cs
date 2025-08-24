using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class RoomsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<RoomType>>("rooms")
            .Arguments(new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "limit" },
                    new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }
            ))
            .Resolve(context =>
        {
            var limit = context.GetArgument<int?>("limit");
                   var documentId = context.GetArgument<long?>("documentId");
            return RevitDispatcher.Invoke(() =>
            {
                       var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
                if (doc == null) return new List<RoomDto>();
                var list = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<SpatialElement>()
                    .Select(r => new RoomDto
                    {
                        Id = r.Id?.Value ?? 0,
                        Name = r.Name,
                        Number = r.Number,
                        Parameters = r.Parameters
                            .Cast<Parameter>()
                            .Where(p => p != null && p.Definition != null)
                            .Select(p => new ParameterDto
                            {
                                Name = p.Definition!.Name,
                                Value = p.AsValueString(),
                            })
                            .ToList()
                    })
                    .ToList();
                if (limit.HasValue) list = list.Take(limit.Value).ToList();
                return list;
            });
        });
    }
}
