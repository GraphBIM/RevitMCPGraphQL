using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Queries;

namespace RevitMCPGraphQL.GraphQL;

internal class RevitQueryProvider
{
    private readonly Func<Document?> _getDoc;

    public RevitQueryProvider(Func<Document?> getDoc)
    {
        _getDoc = getDoc;
    }

    public IObjectGraphType GetQuery()
    {
        var query = new ObjectGraphType
        {
            // Important: give the root query a unique name to avoid duplicate 'Object' types
            Name = "Query"
        };

        // Register modular query contributors
        var contributors = new IQueryContributor[]
        {
            new Queries.HealthQueryContributor(),
            new Queries.ModelHealthQueryContributor(),
            new Queries.DocumentQueryContributor(),
            new Queries.CategoriesQueryContributor(),
            new Queries.ElementsQueryContributor(),
            new Queries.FamilyTypesQueryContributor(),
            new Queries.RoomsQueryContributor(),
            new Queries.LevelsQueryContributor(),
            new Queries.ViewsQueryContributor(),
            new Queries.FamiliesQueryContributor(),
            new Queries.FamilyInstancesQueryContributor(),
            new Queries.ProjectInfoQueryContributor(),
            new Queries.ElementsByIdQueryContributor(),
            new Queries.MaterialsQueryContributor(),
            new Queries.WorksetsQueryContributor(),
            new Queries.PhasesQueryContributor(),
            new Queries.DesignOptionsQueryContributor(),
            new Queries.LinksQueryContributor(),
            new Queries.SheetsQueryContributor(),
            new Queries.SchedulesQueryContributor(),
            new Queries.GridsQueryContributor(),
            new Queries.ProjectLocationQueryContributor(),
            new Queries.WarningsQueryContributor(),
            new Queries.ElementTypesQueryContributor(),
            new Queries.ElementsByCategoryQueryContributor(),
            new Queries.ElementRelationshipQueryContributor(),
            new Queries.ActiveViewAndSelectionQueryContributor(),
            new Queries.ElementsInBoundingBoxQueryContributor(),
            new Queries.UnitsQueryContributor(),
            new Queries.CoordinatesQueryContributor()
        };

        foreach (var c in contributors)
            c.Register(query, _getDoc);

        return query;
    }
}