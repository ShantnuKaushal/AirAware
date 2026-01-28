using AirAware.API.Data;
using AirAware.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirAware.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly FlightIngestionService _ingestionService;
        private readonly AppDbContext _context;

        public FlightsController(FlightIngestionService ingestionService, AppDbContext context)
        {
            _ingestionService = ingestionService;
            _context = context;
        }

        // POST: api/flights/sync
        [HttpPost("sync")]
        public async Task<IActionResult> SyncFlights()
        {
            try 
            {
                int count = await _ingestionService.FetchAndSaveFlightsAsync();
                return Ok(new { Message = $"Successfully synced {count} new flights." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // GET: api/flights
        [HttpGet]
        public async Task<IActionResult> GetFlights()
        {
            var flights = await _context.Flights
                .Include(f => f.StressReport) 
                .OrderByDescending(f => f.Id) 
                .ToListAsync();
                
            return Ok(flights);
        }
    }
}