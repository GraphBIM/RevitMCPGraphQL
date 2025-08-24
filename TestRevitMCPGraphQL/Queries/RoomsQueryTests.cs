using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class RoomsQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Rooms_Works()
    {
        const string q = "query { rooms { id name number area } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["rooms"], Is.Not.Null);
    }

    [Test]
    public async Task Rooms_WithLimit_Works()
    {
        const string q = "query($limit:Int){ rooms(limit:$limit){ id } }";
        var data = await PostGraphQLAsync(q, new { limit = 2 });
        Assert.That(data, Is.Not.Null);
        var arr = data!["rooms"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.LessThanOrEqualTo(2));
    }
}
