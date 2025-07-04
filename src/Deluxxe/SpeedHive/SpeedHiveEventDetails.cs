
using System.Text.Json.Serialization;

namespace Deluxxe.SpeedHive;

public class SpeedHiveEventDetails
{
    [JsonPropertyName("sessions")]
    public required SpeedHiveSessions Sessions { get; set; }
}

public class SpeedHiveSessions
{
    [JsonPropertyName("groups")]
    public List<SpeedHiveGroup> Groups { get; set; } = [];
}

public class SpeedHiveGroup
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("sessions")]
    public List<SpeedHiveSession> Sessions { get; set; } = [];
}

public class SpeedHiveSession
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }
}
