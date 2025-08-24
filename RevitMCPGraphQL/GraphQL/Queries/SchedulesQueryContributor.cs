using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;
using GraphQL;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class SchedulesQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ListGraphType<ScheduleType>>("schedules")
                .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }))
            .Resolve(ctx => RevitDispatcher.Invoke(() =>
            {
                    var documentId = ctx.GetArgument<long?>("documentId");
                    var doc = DocumentResolver.ResolveDocument(getDoc(), documentId);
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
