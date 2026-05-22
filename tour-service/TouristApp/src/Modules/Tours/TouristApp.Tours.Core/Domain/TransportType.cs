using System.Text.Json.Serialization;

namespace TouristApp.Tours.Core.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransportType
{
    Walking = 0,
    Bicycle = 1,
    Car = 2
}