using AirAware.API.Data;
using AirAware.Shared.Protos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Setup Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Register Flight Service (for AviationStack)
builder.Services.AddHttpClient<AirAware.API.Services.FlightIngestionService>();
builder.Services.AddScoped<AirAware.API.Services.FlightIngestionService>();

// 4. Register gRPC Client (Connects to Weather Service)
builder.Services.AddGrpcClient<WeatherProcessor.WeatherProcessorClient>(o =>
{
    o.Address = new Uri("http://localhost:5119");
});

var app = builder.Build();

// Configure the pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();