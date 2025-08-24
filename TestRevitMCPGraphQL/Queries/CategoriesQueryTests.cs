using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class CategoriesQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Categories_WithLimit_Works()
    {
        const string q = "query($limit:Int){ categories(limit:$limit){ id name } }";
        var data = await PostGraphQlAsync(q, new { limit = 5 });
        Assert.That(data, Is.Not.Null);
        var arr = data!["categories"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Console.WriteLine(data);
    }
}
