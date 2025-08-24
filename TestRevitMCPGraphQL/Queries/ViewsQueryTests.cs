using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class ViewsQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Views_Works()
    {
        const string q = "query { views(limit:5) { id name viewType isTemplate } }";
        var data = await PostGraphQlAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["views"], Is.Not.Null);
    }
}
