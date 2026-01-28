using AirAware.Weather.Services;
using AirAware.Weather.Grpc; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Add gRPC
builder.Services.AddGrpc(); 

builder.Services.AddHttpClient<WeatherLogicService>();
builder.Services.AddScoped<WeatherLogicService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Turn on the gRPC Endpoint
app.MapGrpcService<GrpcWeatherService>(); 

app.Run();