using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class RotateElementsTests : BaseGraphQLTest
{
    [Test]
    public async Task RotateElements_InvalidIds_ReturnsFalse()
    {
        const string m = "mutation($ids:[ID!]!,$p:PointInput!,$a:VectorInput!,$angle:Float!){ rotateElements(elementIds:$ids, point:$p, axis:$a, angle:$angle) }";
        var data = await PostGraphQlAsync(m, new
        {
            ids = new long[] { -1, -2 },
            p = new { x = 0.0, y = 0.0, z = 0.0 },
            a = new { x = 0.0, y = 0.0, z = 1.0 },
            angle = 0.78539816339 // ~45 deg
        });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["rotateElements"]?.GetValue<bool>(), Is.False);
    }
}
