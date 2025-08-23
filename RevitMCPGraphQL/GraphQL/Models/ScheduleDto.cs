namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class ScheduleDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool? IsTemplate { get; set; }
}
