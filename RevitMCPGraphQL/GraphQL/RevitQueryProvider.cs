using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Queries;
using CategoryType = RevitMCPGraphQL.GraphQL.Types.CategoryType;
using ElementType = RevitMCPGraphQL.GraphQL.Types.ElementType;

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
        var query = new ObjectGraphType();

        // Register modular query contributors
        var contributors = new IQueryContributor[]
        {
            new Queries.HealthQueryContributor(),
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
            new Queries.ElementsByIdQueryContributor()
        };

        foreach (var c in contributors)
            c.Register(query, _getDoc);

        return query;
    }
}