using System.Text.Json;
using AirAware.API.Data;
using AirAware.API.DTOs;
using AirAware.Shared;
using Microsoft.EntityFrameworkCore;

namespace AirAware.API.Services
{
    public class FlightIngestionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public FlightIngestionService(HttpClient httpClient, IConfiguration configuration, AppDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
        }

        public async Task<int> FetchAndSaveFlightsAsync()
        {
            // 1. Get API Key securely
            var apiKey = _configuration["ApiKeys:AviationStack"];
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("API Key missing!");

            // 2. Build the Request URL (Limit to 20 active flights to save quota)
            var url = $"http://api.aviationstack.com/v1/flights?access_key={apiKey}&limit=20&flight_status=active";

            // 3. Call the API
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return 0;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AviationStackResponse>(json);

            if (result?.Data == null) return 0;

            int newFlightsCount = 0;

            // 4. Loop through results and save to DB
            foreach (var item in result.Data)
            {
                // Check if flight already exists to avoid duplicates
                var exists = await _context.Flights.AnyAsync(f => f.FlightIata == item.Flight.Iata);
                if (exists) continue;

                var flight = new Flight
                {
                    FlightIata = item.Flight.Iata ?? "Unknown",
                    Airline = item.Airline.Name ?? "Unknown",
                    OriginAirport = item.Departure.Iata ?? "Unknown",
                    DestinationAirport = item.Arrival.Iata ?? "Unknown",
                    Status = item.FlightStatus,
                    DepartureTime = item.Departure.Scheduled,
                    ArrivalTime = item.Arrival.Estimated
                };

                _context.Flights.Add(flight);
                newFlightsCount++;
            }

            // 5. Commit changes to Postgres
            await _context.SaveChangesAsync();
            return newFlightsCount;
        }
    }
}