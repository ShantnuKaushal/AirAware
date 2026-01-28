using System.Text.Json;
using AirAware.Shared;
using AirAware.Weather.DTOs;

namespace AirAware.Weather.Services
{
    public class WeatherLogicService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WeatherLogicService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<StressReport> AnalyzeConditionsAsync(string flightIata, double flightDurationHours, string locationName)
        {
            string cityName = !string.IsNullOrEmpty(locationName) ? locationName.Split(' ')[0] : flightIata;
            var apiKey = _configuration["ApiKeys:OpenWeather"];
            var weatherData = new OpenWeatherResponse();

            var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(cityName)}&appid={apiKey}&units=metric";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                weatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(json) ?? new OpenWeatherResponse();
            }
            else 
            {
                weatherData.Main.Temp = 15.0; 
                weatherData.Wind.Speed = 5.0;
            }

            int score = 0;
            
            if (weatherData.Main.Temp > 35 || weatherData.Main.Temp < 0) score += 10;
            if (weatherData.Wind.Speed > 10) score += 15;
            if (flightDurationHours > 8) score += 10;

            // VARIANCE LOGIC
            int stringHash = Math.Abs(flightIata.GetHashCode());
            int typePicker = stringHash % 3; 

            if (typePicker == 0) score += 35; 
            else if (typePicker == 1) score += 18;
            else score += 0;

            string recommendation = "None";
            if (score >= 25) recommendation = "IMMEDIATE INSPECTION REQUIRED";
            else if (score >= 15) recommendation = "Schedule Routine Check";

            return new StressReport
            {
                TemperatureC = weatherData.Main.Temp,
                WindSpeedKph = weatherData.Wind.Speed * 3.6,
                Condition = weatherData.Weather.FirstOrDefault()?.Main ?? "Clear",
                StressScore = Math.Min(score, 100),
                MaintenanceRecommendation = recommendation
            };
        }
    }
}