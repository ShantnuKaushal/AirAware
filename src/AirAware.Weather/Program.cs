using AirAware.Weather.Services;
using AirAware.Weather.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core; 

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // Port 5119: Keep using HTTP 1.1 for Swagger/Browser
    options.ListenLocalhost(5119, o => o.Protocols = HttpProtocols.Http1);

    // Port 5222: NEW Port specifically for gRPC (HTTP 2)
    options.ListenLocalhost(5222, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
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
app.MapGrpcService<GrpcWeatherService>();

app.Run();