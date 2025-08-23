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

    [Test]
    public async Task Elements_WithLimit_Works()
    {
        const string q = "query($limit:Int){ elements(limit:$limit){ id } }";
        var data = await PostGraphQLAsync(q, new { limit = 2 });
        Assert.That(data, Is.Not.Null);
        var arr = data!["elements"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.LessThanOrEqualTo(2));
    }

    [Test]
    public async Task Rooms_WithLimit_Works()
    {
        const string q = "query($limit:Int){ rooms(limit:$limit){ id } }";
        var data = await PostGraphQLAsync(q, new { limit = 2 });
        Assert.That(data, Is.Not.Null);
        var arr = data!["rooms"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.LessThanOrEqualTo(2));
    }

    [Test]
    public async Task Levels_Works()
    {
        const string q = "query { levels { id name elevation } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["levels"], Is.Not.Null);
    }

    [Test]
    public async Task Views_Works()
    {
        const string q = "query { views(limit:5) { id name viewType isTemplate } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["views"], Is.Not.Null);
    }

    [Test]
    public async Task Families_Works()
    {
        const string q = "query { families(limit:5) { id name categoryName } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["families"], Is.Not.Null);
    }

    [Test]
    public async Task FamilyInstances_Works()
    {
        const string q = "query { familyInstances(limit:5) { id name familyName typeName categoryName levelId } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["familyInstances"], Is.Not.Null);
    }

    [Test]
    public async Task ProjectInfo_Works()
    {
        const string q = "query { projectInfo { projectName projectNumber organizationName buildingName } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["projectInfo"], Is.Not.Null);
    }

    [Test]
    public async Task ElementsById_Works_WhenEmpty()
    {
        const string q = "query($ids:[Int]) { elementsById(ids:$ids){ id name } }";
        var data = await PostGraphQLAsync(q, new { ids = Array.Empty<int>() });
        Assert.That(data, Is.Not.Null);
        var arr = data!["elementsById"]?.AsArray();
        Assert.That(arr, Is.Not.Null);
        Assert.That(arr!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Materials_WithLimit_Works()
    {
        const string q = "query($limit:Int){ materials(limit:$limit){ id name class appearanceAssetName } }";
        var data = await PostGraphQLAsync(q, new { limit = 3 });
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["materials"], Is.Not.Null);
    }

    [Test]
    public async Task Worksets_Works()
    {
        const string q = "query { worksets { id name kind } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["worksets"], Is.Not.Null);
    }

    [Test]
    public async Task Phases_Works()
    {
        const string q = "query { phases { id name } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["phases"], Is.Not.Null);
    }

    [Test]
    public async Task ElementTypes_Works()
    {
        const string q = "query { elementTypes { id name familyName categoryName } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["elementTypes"], Is.Not.Null);
    }

    [Test]
    public async Task ElementsByCategory_Works()
    {
        const string q = "query { elementsByCategory(category: OST_Walls, limit: 2) { id name } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["elementsByCategory"], Is.Not.Null);
    }

    [Test]
    public async Task ElementsInBoundingBox_Works()
    {
        const string q = "query { elementsInBoundingBox(minX:-100000,minY:-100000,minZ:-100000,maxX:100000,maxY:100000,maxZ:100000, limit: 2) { id name } }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!["elementsInBoundingBox"], Is.Not.Null);
    }

    [Test]
    public async Task ActiveView_And_Selection_Works()
    {
        const string q = "query { activeView { id name viewType isTemplate } selection }";
        var data = await PostGraphQLAsync(q);
        Assert.That(data, Is.Not.Null);
        // activeView can be null for some states, selection is always an array
        Assert.That(data!["selection"], Is.Not.Null);
    }
}
