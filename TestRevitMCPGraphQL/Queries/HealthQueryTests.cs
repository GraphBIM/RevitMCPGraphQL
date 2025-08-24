using System.Text.Json.Nodes;
using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class HealthQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Health_ReturnsOk()
    {
        const string q = "query { health }";
        var data = await PostGraphQlAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["health"]?.GetValue<string>(), Is.EqualTo("ok"));
    }
}
