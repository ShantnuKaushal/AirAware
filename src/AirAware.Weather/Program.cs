using AirAware.Weather.Services;
using AirAware.Weather.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5119, o => o.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(5222, o => o.Protocols = HttpProtocols.Http2);
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

app.MapControllers();
app.MapGrpcService<GrpcWeatherService>();

app.Run();