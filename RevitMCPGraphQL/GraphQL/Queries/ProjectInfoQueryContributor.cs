using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ProjectInfoQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ProjectInfoType>("projectInfo")
            .Resolve(_ =>
            {
                return RevitDispatcher.Invoke(() =>
                {
                    var doc = getDoc();
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
