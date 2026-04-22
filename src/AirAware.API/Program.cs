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
    o.Address = new Uri("http://weather-service:5222");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

await ApplyDatabaseMigrationsAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.MapControllers();

await app.RunAsync();

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 15;
    var delay = TimeSpan.FromSeconds(2);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            logger.LogInformation("Applying database migrations. Attempt {Attempt} of {MaxAttempts}.", attempt, maxAttempts);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
            logger.LogWarning(ex, "Database migration attempt {Attempt} failed. Retrying in {DelaySeconds} seconds.", attempt, delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }

    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
        logger.LogError("Database migrations failed after {MaxAttempts} attempts.", maxAttempts);
    }

    throw new InvalidOperationException($"Failed to apply database migrations after {maxAttempts} attempts.");
}
