using System.Collections.Generic;

namespace RevitMCPGraphQL.GraphQL.Models;

public sealed class CoordinatesDto
{
    public BasePointDto? ProjectBasePoint { get; set; }
    public BasePointDto? SharedBasePoint { get; set; }
    public List<BasePointDto> AllBasePoints { get; set; } = new();
}
