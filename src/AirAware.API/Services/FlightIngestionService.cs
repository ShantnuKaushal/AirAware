using System.Text.Json;
using AirAware.API.Data;
using AirAware.API.DTOs;
using AirAware.Shared;
using AirAware.Shared.Protos; 
using Microsoft.EntityFrameworkCore;

namespace AirAware.API.Services
{
    public class FlightIngestionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly WeatherProcessor.WeatherProcessorClient _weatherClient; 

        public FlightIngestionService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            AppDbContext context,
            WeatherProcessor.WeatherProcessorClient weatherClient) 
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _weatherClient = weatherClient;
        }

        public async Task<int> FetchAndSaveFlightsAsync()
        {
            // 1. Get API Key
            var apiKey = _configuration["ApiKeys:AviationStack"];
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("API Key missing!");

            // 2. Build URL
            var url = $"http://api.aviationstack.com/v1/flights?access_key={apiKey}&limit=20&flight_status=active";

            // 3. Call AviationStack
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return 0;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AviationStackResponse>(json);

            if (result?.Data == null) return 0;

            int newFlightsCount = 0;

            // 4. Loop through flights
            foreach (var item in result.Data)
            {
                // Skip if exists
                var exists = await _context.Flights.AnyAsync(f => f.FlightIata == item.Flight.Iata);
                if (exists) continue;

                // Create Flight Object
                var flight = new Flight
                {
                    FlightIata = item.Flight.Iata ?? "Unknown",
                    Airline = item.Airline.Name ?? "Unknown",
                    OriginAirport = item.Departure.Iata ?? "Unknown",
                    DestinationAirport = item.Arrival.Iata ?? "Unknown",
                    Status = item.FlightStatus,
                    DepartureTime = item.Departure.Scheduled.ToUniversalTime(),
                    ArrivalTime = item.Arrival.Estimated?.ToUniversalTime()
                };

                try 
                {
                    // Calculate Duration (Default to 2 hours if data missing)
                    double duration = 2.0;
                    if (flight.ArrivalTime.HasValue && flight.DepartureTime != DateTime.MinValue)
                    {
                        duration = (flight.ArrivalTime.Value - flight.DepartureTime).TotalHours;
                        if (duration < 0) duration = 0; // Fix negative times
                    }

                    // Create gRPC Request
                    var weatherRequest = new WeatherRequest 
                    { 
                        AirportCode = flight.DestinationAirport, 
                        FlightDuration = duration 
                    };
                    
                    var weatherReply = await _weatherClient.GetStressScoreAsync(weatherRequest);

                    // Create Stress Report from the reply
                    var stressReport = new StressReport
                    {
                        TemperatureC = weatherReply.Temperature,
                        WindSpeedKph = weatherReply.WindSpeed,
                        Condition = weatherReply.Recommendation, 
                        StressScore = weatherReply.StressScore,
                        MaintenanceRecommendation = weatherReply.Recommendation,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Save Stress Report & Link to Flight
                    _context.StressReports.Add(stressReport);
                    flight.StressReport = stressReport;
                }
                catch (Exception ex)
                {
                    // If Weather Service is down, log it but don't crash.
                    // The flight will save without a stress report.
                    Console.WriteLine($"Weather Service Error: {ex.Message}");
                }

                _context.Flights.Add(flight);
                newFlightsCount++;
            }

            await _context.SaveChangesAsync();
            return newFlightsCount;
        }
    }
}