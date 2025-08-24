using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class FamiliesQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Families_Works()
    {
        const string q = "query { families(limit:5) { id name categoryName } }";
        var data = await PostGraphQlAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["families"], Is.Not.Null);
        Console.WriteLine(data);
    }
}
