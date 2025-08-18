// File: `RevitMCPGraphQL/GraphQL/Models/ParameterDto.cs`
namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class ParameterDto
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
}