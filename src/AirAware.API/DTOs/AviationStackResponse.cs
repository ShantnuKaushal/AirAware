using System.Text.Json.Serialization;

namespace AirAware.API.DTOs
{
    // The Root response from the API
    public class AviationStackResponse
    {
        [JsonPropertyName("data")]
        public List<FlightData> Data { get; set; } = new();
    }

    // The specific details of one flight
    public class FlightData
    {
        [JsonPropertyName("flight_date")]
        public string FlightDate { get; set; } = string.Empty;

        [JsonPropertyName("flight_status")]
        public string FlightStatus { get; set; } = string.Empty;

        [JsonPropertyName("departure")]
        public Departure Departure { get; set; } = new();

        [JsonPropertyName("arrival")]
        public Arrival Arrival { get; set; } = new();

        [JsonPropertyName("flight")]
        public FlightInfo Flight { get; set; } = new();
        
        [JsonPropertyName("airline")]
        public AirlineInfo Airline { get; set; } = new();
    }

    public class Departure
    {
        [JsonPropertyName("airport")]
        public string Airport { get; set; } = string.Empty;
        
        [JsonPropertyName("iata")]
        public string Iata { get; set; } = string.Empty;

        [JsonPropertyName("scheduled")]
        public DateTime Scheduled { get; set; }
    }

    public class Arrival
    {
        [JsonPropertyName("airport")]
        public string Airport { get; set; } = string.Empty;

        [JsonPropertyName("iata")]
        public string Iata { get; set; } = string.Empty;

        [JsonPropertyName("estimated")]
        public DateTime? Estimated { get; set; }
    }

    public class FlightInfo
    {
        [JsonPropertyName("iata")]
        public string Iata { get; set; } = string.Empty;
    }

    public class AirlineInfo 
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}