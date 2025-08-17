using Newtonsoft.Json.Linq;

namespace RevitMCPGraphQL.GraphQL;

internal class GraphQlHttpRequest
{
    public string? Query { get; set; }
    public JObject? Variables { get; set; }
}
