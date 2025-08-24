using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class DuplicateViewTests : BaseGraphQLTest
{
    [Test]
    public async Task DuplicateView_InvalidId_ReturnsNull()
    {
        const string m = "mutation($id:ID!,$d:Boolean){ duplicateView(viewId:$id, withDetailing:$d) }";
        var data = await PostGraphQLAsync(m, new { id = -1, d = false });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["duplicateView"]?.GetValue<string?>(), Is.Null);
    }
}
