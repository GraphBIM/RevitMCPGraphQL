using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class PhasesQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Phases_Works()
    {
        const string q = "query { phases { id name } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["phases"], Is.Not.Null);
    }
}
