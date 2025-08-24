using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class ElementsByIdQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task ElementsById_Works_WhenEmpty()
    {
        const string q = "query($ids:[Int]) { elementsById(ids:$ids){ id name } }";
        var data = await PostGraphQLAsync(q, new { ids = Array.Empty<int>() });
        Assert.That(data, Is.Not.Null);
        var arr = data!["elementsById"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.EqualTo(0));
    }
}
