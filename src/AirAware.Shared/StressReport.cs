using System.ComponentModel.DataAnnotations;

namespace AirAware.Shared
{
    public class StressReport
    {
        [Key]
        public int Id { get; set; }

        public double TemperatureC { get; set; }
        public double WindSpeedKph { get; set; }
        public string Condition { get; set; } = string.Empty; // e.g. "Thunderstorm"

        // The Logic Engine Result
        public int StressScore { get; set; } // 0 to 100
        public string MaintenanceRecommendation { get; set; } = "None"; // "Inspect Gear", "Check Engine"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}