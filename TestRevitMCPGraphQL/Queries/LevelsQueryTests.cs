using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class LevelsQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Levels_Works()
    {
        const string q = "query { levels { id name elevation } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["levels"], Is.Not.Null);
    }
}
