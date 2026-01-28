using System.Text.Json.Serialization;

namespace AirAware.Weather.DTOs
{
    public class OpenWeatherResponse
    {
        [JsonPropertyName("main")]
        public MainData Main { get; set; } = new();

        [JsonPropertyName("wind")]
        public WindData Wind { get; set; } = new();

        [JsonPropertyName("weather")]
        public List<WeatherDescription> Weather { get; set; } = new();
    }

    public class MainData
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; } // Celsius
    }

    public class WindData
    {
        [JsonPropertyName("speed")]
        public double Speed { get; set; } // Meters per second
    }

    public class WeatherDescription
    {
        [JsonPropertyName("main")]
        public string Main { get; set; } = string.Empty; // e.g. "Rain", "Clear"
    }
}