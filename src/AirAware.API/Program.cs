using AirAware.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Tell .NET to use Controllers
builder.Services.AddControllers();

// 2. Setup Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Setup Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Register our Flight Service
builder.Services.AddHttpClient<AirAware.API.Services.FlightIngestionService>();
builder.Services.AddScoped<AirAware.API.Services.FlightIngestionService>();

var app = builder.Build();

// Configure the pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 5. Map the Controllers so the API works
app.MapControllers();

app.Run();