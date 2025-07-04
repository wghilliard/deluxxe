
using System.Text.Json.Serialization;

namespace Deluxxe.SpeedHive;

public class SpeedHiveEvent
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("startDate")]
    public required string StartDate { get; set; }

    [JsonPropertyName("organization")]
    public required SpeedHiveOrganization Organization { get; set; }
}

public class SpeedHiveOrganization
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
