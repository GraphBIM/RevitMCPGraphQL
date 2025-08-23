using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Executes;

internal interface IMutationContributor
{
    void Register(ObjectGraphType mutation, Func<Autodesk.Revit.DB.Document?> getDoc);
}
