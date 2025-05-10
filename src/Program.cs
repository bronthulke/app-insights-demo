using System.Diagnostics;
using AppInsightsDemo.Data;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Add Application Insights (read connection string from configuration or environment)
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    // options.ConnectionString = "YOUR_CONNECTION_STRING"; // set a static connection string
    
    // BETTER: Try to get from environment variable first, then from configuration (or secret store)
    var connectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")
        ?? builder.Configuration["ApplicationInsights:ConnectionString"];

    if (!string.IsNullOrEmpty(connectionString))
    {
        options.ConnectionString = connectionString;
    }
});

// Optionally tag environment
builder.Services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameTelemetryInitializer("blazor-demo"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

public class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
{
    private readonly string _roleName;
    public CloudRoleNameTelemetryInitializer(string roleName) => _roleName = roleName;
    public void Initialize(Microsoft.ApplicationInsights.Channel.ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = _roleName;
        telemetry.Context.GlobalProperties["environment"] = "Development"; // Or "Production"
    }
}

