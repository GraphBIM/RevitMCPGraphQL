using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class ProjectInfoQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task ProjectInfo_Works()
    {
        const string q = "query { projectInfo { projectName projectNumber organizationName buildingName } }";
        var data = await PostGraphQlAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["projectInfo"], Is.Not.Null);
    }
}
