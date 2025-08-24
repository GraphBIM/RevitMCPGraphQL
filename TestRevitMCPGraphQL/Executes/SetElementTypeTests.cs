using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class SetElementTypeTests : BaseGraphQLTest
{
    [Test]
    public async Task SetElementType_InvalidIds_ReturnsFalse()
    {
        const string m = "mutation($elementId:ID!,$typeId:ID!){ setElementType(elementId:$elementId, typeId:$typeId) }";
        var data = await PostGraphQlAsync(m, new { elementId = -1, typeId = -1 });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["setElementType"]?.GetValue<bool>(), Is.False);
    }
}
