using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class ActiveViewAndSelectionQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task ActiveView_And_Selection_Works()
    {
        const string q = "query { activeView { id name viewType isTemplate } selection }";
        var data = await PostGraphQlAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["selection"], Is.Not.Null);
        Console.WriteLine(data);
    }
}
