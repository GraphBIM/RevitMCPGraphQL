using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Executes;

namespace RevitMCPGraphQL.GraphQL;

internal sealed class RevitMutationProvider
{
    private readonly Func<Autodesk.Revit.DB.Document?> _getDoc;

    public RevitMutationProvider(Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        _getDoc = getDoc;
    }

    public IObjectGraphType GetMutation()
    {
        var mutation = new ObjectGraphType();

        var contributors = new IMutationContributor[]
        {
            new SetElementParametersBatchContributor(),
        };

        foreach (var c in contributors)
            c.Register(mutation, _getDoc);

        return mutation;
    }
}
