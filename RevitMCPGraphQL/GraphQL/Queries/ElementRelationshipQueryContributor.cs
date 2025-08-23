using GraphQL;
using GraphQL.Types;
using RevitMCPGraphQL.GraphQL.Models;
using RevitMCPGraphQL.GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal sealed class ElementRelationshipQueryContributor : IQueryContributor
{
    public void Register(ObjectGraphType query, Func<Autodesk.Revit.DB.Document?> getDoc)
    {
        query.Field<ElementRelationshipType>("elementRelationship")
            .Description("Get relationships for an element: super component, host, dependents, joined elements.")
            .Arguments(new QueryArguments(
                new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "elementId" }
            ))
            .Resolve(ctx =>
            {
                var elementId = ctx.GetArgument<long>("elementId");
                return RevitDispatcher.Invoke(() =>
                {
                    var doc = getDoc();
                    if (doc == null) return (object?)null;
                    var el = doc.GetElement(new ElementId(elementId));
                    if (el == null) return (object?)null;

                    long? superId = (el as FamilyInstance)?.SuperComponent?.Id?.Value;
                    long? hostId = (el as FamilyInstance)?.Host?.Id?.Value;

                    var dependents = new List<long>();
                    try
                    {
                        var ids = el.GetDependentElements(null);
                        if (ids != null)
                            dependents.AddRange(ids.Select(i => (long)i.Value));
                    }
                    catch { }

                    var joined = new List<long>();
                    try
                    {
                        var jids = JoinGeometryUtils.GetJoinedElements(doc, el);
                        if (jids != null)
                            joined.AddRange(jids.Select(i => (long)i.Value));
                    }
                    catch { }

                    return new ElementRelationshipDto
                    {
                        ElementId = elementId,
                        SuperComponentId = superId,
                        HostId = hostId,
                        DependentIds = dependents.Distinct().ToList(),
                        JoinedIds = joined.Distinct().ToList(),
                    };
                });
            });
    }
}
