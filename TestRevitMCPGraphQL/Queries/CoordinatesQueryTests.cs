using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class CoordinatesQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Coordinates_Works()
    {
        const string q = "query { coordinates { projectBasePoint { x y z } sharedBasePoint { x y z } allBasePoints { id name isShared x y z } } }";
        var data = await PostGraphQlAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["coordinates"], Is.Not.Null);
        Console.WriteLine(data);
    }
}
