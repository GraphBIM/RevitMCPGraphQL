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
        var mutation = new ObjectGraphType
        {
            // Important: give the root mutation a unique name to avoid duplicate 'Object' types
            Name = "Mutation"
        };

        var contributors = new IMutationContributor[]
        {
            new SetElementParametersBatchContributor(),
            new SetElementParameterContributor(),
            new SetElementTypeContributor(),
            new MoveElementsContributor(),
            new RotateElementsContributor(),
            new DeleteElementsContributor(),
            new CreateFamilyInstanceContributor(),
            new DuplicateViewContributor(),
            new CreateSheetContributor(),
        };

        foreach (var c in contributors)
            c.Register(mutation, _getDoc);

        return mutation;
    }
}
