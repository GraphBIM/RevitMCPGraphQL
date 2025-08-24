using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class ElementsQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Elements_NoFilter_Works()
    {
        // parameters is now a map scalar, so select it directly
        const string q = "query { elements { id name parameters } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["elements"], Is.Not.Null);
        Console.WriteLine(data);
    }

    [Test]
    public async Task Elements_WithLimit_Works()
    {
        const string q = "query($limit:Int){ elements(limit:$limit){ id } }";
        var data = await PostGraphQLAsync(q, new { limit = 2 });
        Assert.That(data, Is.Not.Null);
        var arr = data!["elements"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.LessThanOrEqualTo(2));
    }
}
