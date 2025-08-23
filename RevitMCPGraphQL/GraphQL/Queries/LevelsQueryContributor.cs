using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitLevel = Autodesk.Revit.DB.Level;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class LevelsQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
    query.Field<ListGraphType<RevitMCPGraphQL.GraphQL.Types.LevelType>>("levels")
            .Arguments(new QueryArguments(new QueryArgument<IntGraphType> { Name = "limit" }))
            .Resolve(ctx =>
            {
                var limit = ctx.GetArgument<int?>("limit");
                return RevitDispatcher.Invoke(() =>
                {
                    var doc = getDoc();
                    if (doc == null) return new List<LevelDto>();

                    var levels = new FilteredElementCollector(doc)
                        .OfClass(typeof(RevitLevel))
                        .Cast<RevitLevel>()
                        .Select(l => new LevelDto
                        {
                            Id = l.Id?.Value ?? 0,
                            Name = l.Name,
                            Elevation = l.Elevation,
                            Parameters = l.Parameters
                                .Cast<Parameter>()
                                .Where(p => p?.Definition != null)
                                .Select(p => new ParameterDto { Name = p.Definition!.Name, Value = p.AsValueString() })
                                .ToList()
                        })
                        .OrderBy(l => l.Elevation)
                        .ToList();

                    if (limit.HasValue) levels = levels.Take(limit.Value).ToList();
                    return levels;
                });
            });
    }
}
