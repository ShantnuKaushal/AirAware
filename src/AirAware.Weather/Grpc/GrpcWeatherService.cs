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
            // Call existing logic engine
            var report = await _logicService.AnalyzeConditionsAsync(request.AirportCode, request.FlightDuration);

            // Convert the result into a gRPC message
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