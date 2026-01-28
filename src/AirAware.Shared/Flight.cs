using System.ComponentModel.DataAnnotations;

namespace AirAware.Shared
{
    public class Flight
    {
        [Key]
        public int Id { get; set; }

        public string FlightIata { get; set; } = string.Empty; // e.g., "AA123"
        public string Airline { get; set; } = string.Empty;    // e.g., "American Airlines"
        
        public string OriginAirport { get; set; } = string.Empty; // e.g., "LHR"
        public string DestinationAirport { get; set; } = string.Empty; // e.g., "JFK"
        
        public string Status { get; set; } = "Scheduled"; // Scheduled, Active, Landed
        
        public DateTime DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }

        public int? StressReportId { get; set; }
        public StressReport? StressReport { get; set; }
    }
}