using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace TestRevitMCPGraphQL;

[TestFixture]
public class GraphQLIntegrationTests
{
    private static readonly Uri BaseUri = new("http://localhost:5000/");
    private static readonly Uri GraphQlUri = new(BaseUri, "graphql");
    private static readonly HttpClient Client = new();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Client.Timeout = TimeSpan.FromSeconds(60);
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Quick availability check; mark tests inconclusive if server not running
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, BaseUri);
            using var resp = Client.Send(req);
            if (!resp.IsSuccessStatusCode)
            {
                Assert.Inconclusive($"Server not ready at {BaseUri} (status {(int)resp.StatusCode})");
            }
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"Cannot reach GraphQL server at {BaseUri}: {ex.Message}");
        }
    }

    private static async Task<JsonNode?> PostGraphQLAsync(string query, object? variables = null)
    {
        var payload = new
        {
            query,
            variables
        };
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(GraphQlUri, content);
        var respText = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"HTTP {(int)response.StatusCode}: {respText}");

        var node = JsonNode.Parse(respText);
        Assert.That(node, Is.Not.Null, "Invalid JSON");

        // Fail fast on GraphQL errors
        var errors = node!["errors"];
        if (errors != null)
        {
            Assert.Fail($"GraphQL errors: {errors.ToJsonString()}");
        }

        return node!["data"];
    }

    [Test]
    public async Task Health_ReturnsOk()
    {
        const string q = "query { health }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["health"]?.GetValue<string>(), Is.EqualTo("ok"));
    }

    [Test]
    public async Task Document_ReturnsShape()
    {
        const string q = "query { document { title pathName isFamilyDocument } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        var doc = data!["document"];
        Assert.That(doc, Is.Not.Null, "document should exist");
        // Title is required in schema
        Assert.That(doc!["title"]?.GetValue<string>(), Is.Not.Null);
    }

    [Test]
    public async Task Categories_WithLimit_Works()
    {
        const string q = "query($limit:Int){ categories(limit:$limit){ id name } }";
        var data = await PostGraphQLAsync(q, new { limit = 1 });
        Assert.That(data, Is.Not.Null);
        var arr = data!["categories"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.LessThanOrEqualTo(1));
    }

    [Test]
    public async Task Elements_NoFilter_Works()
    {
        const string q = "query { elements { id name parameters { name value } } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["elements"], Is.Not.Null);
    }

    [Test]
    public async Task FamilyTypes_NoFilter_Works()
    {
        const string q = "query { familyTypes { id name familyName categoryName parameters { name value } } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["familyTypes"], Is.Not.Null);
    }

    [Test]
    public async Task Rooms_Works()
    {
        const string q = "query { rooms { id name number area } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["rooms"], Is.Not.Null);
    }
}
