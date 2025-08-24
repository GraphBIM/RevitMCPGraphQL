using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class MaterialsQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Materials_WithLimit_Works()
    {
        const string q = "query($limit:Int){ materials(limit:$limit){ id name class appearanceAssetName } }";
        var data = await PostGraphQLAsync(q, new { limit = 3 });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["materials"], Is.Not.Null);
    }
}
