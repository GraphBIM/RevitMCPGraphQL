using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class UnitsQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Units_Works()
    {
        const string q = "query { units { name typeId symbol } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["units"], Is.Not.Null);
    }
}
