using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class SetElementParameterTests : BaseGraphQLTest
{
    [Test]
    public async Task SetElementParameter_InvalidId_ReturnsFalse()
    {
        const string m = "mutation($elementId:ID!,$parameterName:String!,$value:String!){ setElementParameter(elementId:$elementId, parameterName:$parameterName, value:$value) }";
        var data = await PostGraphQlAsync(m, new { elementId = -1, parameterName = "Comments", value = "Test" });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["setElementParameter"]?.GetValue<bool>(), Is.False);
    }
}
