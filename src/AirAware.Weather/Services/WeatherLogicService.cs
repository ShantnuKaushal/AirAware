using System.Text.Json;
using AirAware.Shared; // Use the Shared StressReport model
using AirAware.Weather.DTOs;

namespace AirAware.Weather.Services
{
    public class WeatherLogicService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // Simple mapping for the demo
        private readonly Dictionary<string, string> _airportToCity = new()
        {
            { "JFK", "New York" }, { "LHR", "London" }, { "DXB", "Dubai" },
            { "HND", "Tokyo" }, { "SIN", "Singapore" }, { "LAX", "Los Angeles" },
            { "CDG", "Paris" }, { "AMS", "Amsterdam" }, { "FRA", "Frankfurt" }
        };

        public WeatherLogicService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<StressReport> AnalyzeConditionsAsync(string airportCode, double flightDurationHours)
        {
            // 1. Resolve City Name
            string city = _airportToCity.ContainsKey(airportCode) ? _airportToCity[airportCode] : "London"; // Default to London if unknown

            // 2. Fetch Weather
            var apiKey = _configuration["ApiKeys:OpenWeather"];
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";
            
            var response = await _httpClient.GetAsync(url);
            var weatherData = new OpenWeatherResponse();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                weatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(json) ?? new OpenWeatherResponse();
            }

            // 3. CALCULATE STRESS SCORE
            int score = 0;
            List<string> reasons = new();

            // Factor A: Temperature (Heat causes engine wear)
            if (weatherData.Main.Temp > 35) 
            {
                score += 10;
                reasons.Add("Extreme Heat");
            }
            else if (weatherData.Main.Temp < 0)
            {
                score += 10;
                reasons.Add("Freezing Conditions");
            }

            // Factor B: Wind (Structural stress on landing gear)
            if (weatherData.Wind.Speed > 10) // > 10 m/s is roughly 20 knots
            {
                score += 15;
                reasons.Add("High Winds");
            }

            // Factor C: Flight Duration
            if (flightDurationHours > 8)
            {
                score += 5;
                reasons.Add("Long Haul Flight");
            }

            // 4. Generate Recommendation
            string recommendation = "None";
            if (score >= 20) recommendation = "IMMEDIATE INSPECTION REQUIRED";
            else if (score >= 10) recommendation = "Schedule Routine Check";

            return new StressReport
            {
                TemperatureC = weatherData.Main.Temp,
                WindSpeedKph = weatherData.Wind.Speed * 3.6, // Convert m/s to km/h
                Condition = weatherData.Weather.FirstOrDefault()?.Main ?? "Clear",
                StressScore = score,
                MaintenanceRecommendation = recommendation
            };
        }
    }
}