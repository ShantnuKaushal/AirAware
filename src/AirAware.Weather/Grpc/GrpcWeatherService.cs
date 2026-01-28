using AirAware.Shared.Protos;
using AirAware.Weather.Services;
using Grpc.Core;

namespace AirAware.Weather.Grpc
{
    public class GrpcWeatherService : WeatherProcessor.WeatherProcessorBase
    {
        private readonly WeatherLogicService _logicService;

        public GrpcWeatherService(WeatherLogicService logicService)
        {
            _logicService = logicService;
        }

        public override async Task<WeatherReply> GetStressScore(WeatherRequest request, ServerCallContext context)
        {
            // Passing AirportCode as the flight ID to calculate variance
            var report = await _logicService.AnalyzeConditionsAsync(request.AirportCode, request.FlightDuration, request.LocationName);

            return new WeatherReply
            {
                StressScore = report.StressScore,
                Recommendation = report.MaintenanceRecommendation,
                Temperature = report.TemperatureC,
                WindSpeed = report.WindSpeedKph
            };
        }
    }
}