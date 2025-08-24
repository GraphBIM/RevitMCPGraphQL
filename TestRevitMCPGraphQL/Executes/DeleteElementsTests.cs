using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class DeleteElementsTests : BaseGraphQLTest
{
    [Test]
    public async Task DeleteElements_InvalidIds_ReturnsFalse()
    {
        const string m = "mutation($ids:[ID!]!){ deleteElements(elementIds:$ids) }";
        var data = await PostGraphQLAsync(m, new { ids = new long[] { -1, -2, -3 } });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["deleteElements"]?.GetValue<bool>(), Is.False);
    }
}
