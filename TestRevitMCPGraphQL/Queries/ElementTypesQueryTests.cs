using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class ElementTypesQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task ElementTypes_Works()
    {
        const string q = "query { elementTypes { id name familyName categoryName } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["elementTypes"], Is.Not.Null);
    }
}
