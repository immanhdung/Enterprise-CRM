using EnterpriseCRM.API.Middleware;
using EnterpriseCRM.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/crm-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// OpenAPI Documentation (.NET 10)
builder.Services.AddOpenApi();

// Add Clean Architecture Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Configure CORS for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure Middleware Pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment() || true)
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Enterprise CRM API v1")
               .WithTheme(ScalarTheme.BluePlanet)
               .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
    });

    // Provide seamless alias for /swagger
    app.MapGet("/swagger", () => Results.Redirect("/scalar/v1"))
       .ExcludeFromDescription();
}

app.UseCors("AllowFrontend");

app.MapControllers();

Log.Information("Enterprise CRM API started on environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
