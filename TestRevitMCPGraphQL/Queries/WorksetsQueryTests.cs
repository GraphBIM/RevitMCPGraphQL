using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class WorksetsQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Worksets_Works()
    {
        const string q = "query { worksets { id name kind } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["worksets"], Is.Not.Null);
    }
}
