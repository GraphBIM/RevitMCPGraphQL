using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class SetElementParametersBatchTests : BaseGraphQLTest
{
    [Test]
    public async Task SetElementParametersBatch_Empty_ReturnsFalse()
    {
        const string m = "mutation($inputs:[ElementParametersInput!]!){ setElementParametersBatch(inputs:$inputs) }";
        var data = await PostGraphQLAsync(m, new { inputs = Array.Empty<object>() });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["setElementParametersBatch"]?.GetValue<bool>(), Is.False);
    }
}
