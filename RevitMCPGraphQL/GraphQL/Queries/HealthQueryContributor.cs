using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class HealthQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<StringGraphType>("health").Resolve(_ => "ok");
    }
}
