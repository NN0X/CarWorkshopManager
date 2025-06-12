using CarWorkshopManager.Documents;
using CarWorkshopManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Services.Implementations
{
    public class OpenOrdersReportBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OpenOrdersReportBackgroundService> _logger;
        private readonly int _intervalMinutes;
        private readonly string _adminEmail;

        public OpenOrdersReportBackgroundService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<OpenOrdersReportBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _intervalMinutes = int.Parse(configuration["ReportSettings:IntervalMinutes"] ?? "1440");
            _adminEmail = configuration["ReportSettings:AdminEmail"]
                ?? throw new InvalidOperationException("ReportSettings:AdminEmail is not configured");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service starting: interval={Minutes}min, adminEmail={Email}",
                _intervalMinutes, _adminEmail);

            await GenerateAndSendReport(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Waiting {Minutes} minutes until next run", _intervalMinutes);
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
                    await GenerateAndSendReport(stoppingToken);
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background loop, retrying in 1 minute");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Service stopping");
        }

        private async Task GenerateAndSendReport(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var orderSvc = scope.ServiceProvider.GetRequiredService<IServiceOrderService>();
            var emailSvc = scope.ServiceProvider.GetRequiredService<IEmailWithAttachmentSender>();

            _logger.LogInformation("Generating open orders report");
            var openOrders = await orderSvc.GetOpenServiceOrdersAsync();
            if (!openOrders.Any())
            {
                _logger.LogInformation("No open orders found, skipping email");
                return;
            }

            byte[] pdfBytes;
            try
            {
                using var ms = new MemoryStream();
                new OpenOrdersReportDocument(new OpenOrdersReportViewModel
                {
                    GeneratedAt = DateTime.Now,
                    Items = openOrders.Select(o => new OpenOrderItemViewModel
                    {
                        ServiceOrderId = o.Id,
                        OrderNumber = o.OrderNumber,
                        OpenedAt = o.OpenedAt,
                        CustomerName = o.CustomerName,
                        RegistrationNumber = o.RegistrationNumber,
                        Status = o.StatusName
                    }).ToList()
                }).GeneratePdf(ms);
                pdfBytes = ms.ToArray();
                _logger.LogInformation("PDF generated ({Bytes} bytes)", pdfBytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate PDF");
                return;
            }

            try
            {
                var timestamp = DateTime.Now;
                var subject = $"Raport otwartych zleceń – {timestamp:yyyy-MM-dd HH:mm}";
                var html = $"<p>Raport wygenerowany {timestamp:yyyy-MM-dd HH:mm}</p>";
                var fileName = $"open_orders_{timestamp:yyyyMMdd_HHmm}.pdf";

                await emailSvc.SendEmailAsync(_adminEmail, subject, html, pdfBytes, fileName, "application/pdf");
                _logger.LogInformation("Email with report sent to {Email}", _adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send report email");
            }
        }
    }
}
