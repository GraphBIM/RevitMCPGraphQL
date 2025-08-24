using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class MoveElementsTests : BaseGraphQLTest
{
    [Test]
    public async Task MoveElements_EmptyList_ReturnsFalse()
    {
        const string m = "mutation($ids:[ID!]!,$t:VectorInput!){ moveElements(elementIds:$ids, translation:$t) }";
        var data = await PostGraphQLAsync(m, new { ids = Array.Empty<long>(), t = new { x = 0.0, y = 0.0, z = 0.0 } });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["moveElements"]?.GetValue<bool>(), Is.False);
    }
}
