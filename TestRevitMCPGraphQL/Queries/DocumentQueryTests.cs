using NUnit.Framework;
using TestRevitMCPGraphQL.TestUtils;

namespace TestRevitMCPGraphQL.Queries;

[TestFixture]
public class DocumentQueryTests : BaseGraphQLTest
{
    [Test]
    public async Task Document_ReturnsShape()
    {
        const string q = "query { document { title pathName isFamilyDocument } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        var doc = data!["document"];
        Assert.That(doc, Is.Not.Null, "document should exist");
        Assert.That(doc!["title"]?.GetValue<string>(), Is.Not.Null);
        Console.WriteLine(data);
    }
}
