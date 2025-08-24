using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class FamilyTypesQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task FamilyTypes_NoFilter_Works()
    {
        // parameters is now a map scalar, so select it directly
        const string q = "query { familyTypes { id name familyName categoryName parameters } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["familyTypes"], Is.Not.Null);
    }
}
