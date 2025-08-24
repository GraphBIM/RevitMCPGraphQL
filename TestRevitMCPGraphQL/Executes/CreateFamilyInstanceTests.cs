using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class CreateFamilyInstanceTests : BaseGraphQLTest
{
    [Test]
    public async Task CreateFamilyInstance_InvalidSymbol_ReturnsNull()
    {
        const string m = "mutation($symbolId:ID!,$loc:PointInput!){ createFamilyInstance(symbolId:$symbolId, location:$loc) }";
        var data = await PostGraphQLAsync(m, new { symbolId = -1, loc = new { x = 0.0, y = 0.0, z = 0.0 } });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["createFamilyInstance"]?.GetValue<string?>(), Is.Null);
    }
}
