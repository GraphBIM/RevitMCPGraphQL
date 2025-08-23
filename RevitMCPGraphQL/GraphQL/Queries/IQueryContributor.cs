using System;
using Autodesk.Revit.DB;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Queries;

internal interface IQueryContributor
{
    void Register(ObjectGraphType query, Func<Document?> getDoc);
}
