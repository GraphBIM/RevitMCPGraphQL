using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace TestRevitMCPGraphQL.TestUtils;

public abstract class BaseGraphQLTest
{
    protected static readonly Uri BaseUri;
    protected static readonly Uri GraphQlUri;
    protected static readonly HttpClient Client = new();
    // Note: Client is configured in the static constructor to avoid modifying
    // properties after any requests may have started.

    static BaseGraphQLTest()
    {
        var baseUrl = Environment.GetEnvironmentVariable("GRAPHQL_BASE_URL") ?? "http://localhost:5000/";
        var path = Environment.GetEnvironmentVariable("GRAPHQL_PATH") ?? "graphql";
        BaseUri = new Uri(baseUrl);
        GraphQlUri = new Uri(BaseUri, path);

    // Configure default headers once before any requests occur
    Client.DefaultRequestHeaders.Accept.Clear();
    Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    [OneTimeSetUp]
    public void OneTimeSetup_Base()
    {
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

    // Back-compat: some tests may call the older name with capital L
    protected static Task<JsonNode?> PostGraphQLAsync(string query, object? variables = null)
        => PostGraphQlAsync(query, variables);

    protected static async Task<JsonNode?> PostGraphQlAsync(string query, object? variables = null)
    {
        var payload = new { query, variables };
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(GraphQlUri, content);
        var respText = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"HTTP {(int)response.StatusCode}: {respText}");

        var node = JsonNode.Parse(respText);
        Assert.That(node, Is.Not.Null, "Invalid JSON");

        var errors = node!["errors"];
        if (errors != null)
        {
            Assert.Fail($"GraphQL errors: {errors.ToJsonString()}");
        }

        return node!["data"];
    }
}
