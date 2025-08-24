using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class FamilyInstancesQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task FamilyInstances_Works()
    {
        const string q = "query { familyInstances(limit:5) { id name familyName typeName categoryName levelId } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["familyInstances"], Is.Not.Null);
    }
}
