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

        public FlightIngestionService(HttpClient httpClient, IConfiguration configuration, AppDbContext context, WeatherProcessor.WeatherProcessorClient weatherClient)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _weatherClient = weatherClient;
        }

        public async Task<int> FetchAndSaveFlightsAsync()
        {
            var apiKey = _configuration["ApiKeys:AviationStack"];
            var url = $"http://api.aviationstack.com/v1/flights?access_key={apiKey}&limit=10&flight_status=active";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return 0;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AviationStackResponse>(json);
            if (result?.Data == null) return 0;

            int newFlightsCount = 0;

            foreach (var item in result.Data)
            {
                var exists = await _context.Flights.AnyAsync(f => f.FlightIata == item.Flight.Iata);
                if (exists) continue;

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
                    double duration = 2.0;
                    if (flight.ArrivalTime.HasValue && flight.DepartureTime != DateTime.MinValue)
                    {
                        duration = (flight.ArrivalTime.Value - flight.DepartureTime).TotalHours;
                        if (duration < 0) duration = 0;
                    }

                    var uniqueId = flight.FlightIata + flight.DestinationAirport;

                    var weatherRequest = new WeatherRequest 
                    { 
                        AirportCode = uniqueId, 
                        FlightDuration = duration,
                        LocationName = item.Arrival.Airport
                    };
                    
                    var weatherReply = await _weatherClient.GetStressScoreAsync(weatherRequest);

                    var stressReport = new StressReport
                    {
                        TemperatureC = weatherReply.Temperature,
                        WindSpeedKph = weatherReply.WindSpeed,
                        Condition = weatherReply.Recommendation,
                        StressScore = weatherReply.StressScore,
                        MaintenanceRecommendation = weatherReply.Recommendation,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.StressReports.Add(stressReport);
                    flight.StressReport = stressReport;
                }
                catch (Exception ex)
                {
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