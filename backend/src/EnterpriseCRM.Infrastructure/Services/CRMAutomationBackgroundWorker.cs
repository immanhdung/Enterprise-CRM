using EnterpriseCRM.Application.Features.AutomationAndAi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class CRMAutomationBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CRMAutomationBackgroundWorker> _logger;

    public CRMAutomationBackgroundWorker(
        IServiceProvider serviceProvider,
        ILogger<CRMAutomationBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CRM Automation & AI Background Worker started.");

        // Wait 15 seconds after startup before running first scan
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
                var anomalyService = scope.ServiceProvider.GetRequiredService<IAnomalyDetectionService>();

                _logger.LogInformation("Executing scheduled CRM smart alerts & anomaly scans...");

                var triggeredAlerts = await alertService.ScanAndTriggerAlertsAsync(stoppingToken);
                var detectedAnomalies = await anomalyService.RunAnomalyScanAsync(stoppingToken);

                _logger.LogInformation("CRM Automation Scan completed: {Alerts} alert(s) triggered, {Anomalies} anomaly(ies) detected.",
                    triggeredAlerts, detectedAnomalies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during CRM automation background execution.");
            }

            // Run every 1 hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
