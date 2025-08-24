using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class ElementsByCategoryQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task ElementsByCategory_Works()
    {
        const string q = "query { elementsByCategory(category: OST_Walls, limit: 2) { id name } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["elementsByCategory"], Is.Not.Null);
    }
}
