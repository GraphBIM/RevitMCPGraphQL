using GraphQL.Types;
using GraphQL;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.RevitUtils;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ProjectInfoQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ProjectInfoType>("projectInfo")
            .Arguments(new QueryArguments(new QueryArgument<IdGraphType> { Name = "documentId", Description = "Optional: RevitLinkInstance element id. If omitted or invalid, uses the active document." }))
            .Resolve(ctx =>
            {
                return RevitDispatcher.Invoke(() =>
                {
                    var docId = ctx.GetArgument<long?>("documentId");
                    var doc = DocumentResolver.ResolveDocument(getDoc(), docId);
                    if (doc == null) return new ProjectInfoDto();
                    var pi = doc.ProjectInformation;
                    if (pi == null) return new ProjectInfoDto();

                    string? TryGet(string parameterName)
                    {
                        try
                        {
                            var p = pi.LookupParameter(parameterName);
                            return p != null ? (p.AsString() ?? p.AsValueString()) : null;
                        }
                        catch { return null; }
                    }

                    return new ProjectInfoDto
                    {
                        ProjectName = TryGet("Project Name"),
                        ProjectNumber = TryGet("Project Number"),
                        OrganizationName = TryGet("Organization Name"),
                        BuildingName = TryGet("Building Name")
                    };
                });
            });
    }
}
