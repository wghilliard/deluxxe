using System.Text.Json.Serialization;

namespace Deluxxe.Raffles;

[JsonConverter(typeof(JsonStringEnumConverter<DrawingType>))]
public enum DrawingType
{
    Race,
    Event
}