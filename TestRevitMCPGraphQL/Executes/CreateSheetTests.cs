using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Executes;

[TestFixture]
public class CreateSheetTests : BaseGraphQLTest
{
    [Test]
    public async Task CreateSheet_InvalidTitleBlock_ReturnsNull()
    {
        const string m = "mutation($tb:ID){ createSheet(titleBlockTypeId:$tb) }";
        var data = await PostGraphQLAsync(m, new { tb = -1L });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["createSheet"]?.GetValue<string?>(), Is.Null);
    }
}
