using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class RoomsQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Rooms_Works()
    {
        const string q = "query { rooms { id name parameters bbox {minX maxY} elementsInside boundaries } }";
        var data = await PostGraphQlAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["rooms"], Is.Not.Null);
        Console.WriteLine(data);
    }

    [Test]
    public async Task Rooms_WithLimit_Works()
    {
        const string q = "query($limit:Int){ rooms(limit:$limit){ elementsInside } }";
        var data = await PostGraphQlAsync(q, new { limit = 2 });
        Assert.That(data, Is.Not.Null);
        var arr = data!["rooms"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.LessThanOrEqualTo(2));
        Console.WriteLine(arr);
    }
}
