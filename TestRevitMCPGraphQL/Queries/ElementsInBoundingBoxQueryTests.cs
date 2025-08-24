using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class ElementsInBoundingBoxQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task ElementsInBoundingBox_Works()
    {
        const string q = "query { elementsInBoundingBox(minX:-100000,minY:-100000,minZ:-100000,maxX:100000,maxY:100000,maxZ:100000, limit: 2) { id name } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["elementsInBoundingBox"], Is.Not.Null);
        Console.WriteLine(data);
    }
}
