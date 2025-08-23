using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class SchedulesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<ScheduleType>>("schedules")
            .Resolve(_ => RevitDispatcher.Invoke(() =>
            {
                var doc = getDoc();
                if (doc == null) return new List<ScheduleDto>();

                var schedules = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSchedule))
                    .Cast<ViewSchedule>()
                    .Where(vs => !vs.IsTitleblockRevisionSchedule) // typical schedules only
                    .Select(vs => new ScheduleDto
                    {
                        Id = vs.Id?.Value ?? 0,
                        Name = vs.Name,
                        IsTemplate = vs.IsTemplate
                    })
                    .OrderBy(s => s.Name)
                    .ToList();

                return schedules;
            }));
    }
}
