using AirAware.Shared;
using Microsoft.EntityFrameworkCore;

namespace AirAware.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<StressReport> StressReports { get; set; }
    }
}